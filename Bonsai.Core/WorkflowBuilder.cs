using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using System.Xml.Serialization;
using System.IO;
using Bonsai.Expressions;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Schema;
using System.Xml;
using System.Diagnostics;
using System.Reflection.Emit;
using Bonsai.Properties;
using System.Xml.Xsl;
using System.ComponentModel;
using System.Globalization;
using Microsoft.CSharp;
using System.CodeDom;

namespace Bonsai
{
    /// <summary>
    /// Represents an XML serializable expression builder workflow container.
    /// </summary>
    public class WorkflowBuilder : IXmlSerializable
    {
        readonly ExpressionBuilderGraph workflow;
        const string DynamicAssemblyPrefix = "@Dynamic";
        const string VersionAttributeName = "Version";
        const string TypeArgumentsAttributeName = "TypeArguments";
        const string ExtensionTypeNodeName = "ExtensionTypes";
        const string DescriptionElementName = "Description";
        const string WorkflowNodeName = "Workflow";
        const string TypeNodeName = "Type";

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowBuilder"/> class.
        /// </summary>
        public WorkflowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowBuilder"/> class with the
        /// specified workflow instance.
        /// </summary>
        /// <param name="workflow">
        /// The <see cref="ExpressionBuilderGraph"/> that will be used by this builder.
        /// </param>
        public WorkflowBuilder(ExpressionBuilderGraph workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        /// <summary>
        /// Gets or sets a description for the serializable workflow.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the <see cref="ExpressionBuilderGraph"/> instance used by this builder.
        /// </summary>
        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

        /// <summary>
        /// Gets a <see cref="XmlSerializer"/> instance that can be used to serialize
        /// or deserialize a <see cref="WorkflowBuilder"/>.
        /// </summary>
        public static XmlSerializer Serializer
        {
            get { return SerializerFactory.instance; }
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement(typeof(WorkflowBuilder).Name);

            if (reader.IsStartElement(DescriptionElementName))
            {
                Description = reader.ReadElementContentAsString();
            }

            var types = new HashSet<Type>();
            var workflowMarkup = string.Empty;
            if (reader.IsStartElement(WorkflowNodeName))
            {
                if (reader.NamespaceURI != Constants.XmlNamespace)
                {
                    workflowMarkup = ConvertDescriptorMarkup(reader.ReadOuterXml());
                }
                else workflowMarkup = ReadXmlExtensions(reader, types);
            }

            XmlSerializer serializer;
            if (reader.ReadToNextSibling(ExtensionTypeNodeName))
            {
                reader.ReadStartElement();
                while (reader.ReadToNextSibling(TypeNodeName))
                {
                    var typeName = reader.ReadElementString();
                    var type = LookupType(typeName);
                    var proxyTypeAttribute = (ProxyTypeAttribute)Attribute.GetCustomAttribute(type, typeof(ProxyTypeAttribute));
                    if (proxyTypeAttribute != null) type = proxyTypeAttribute.Type;
                    types.Add(type);
                }

                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == ExtensionTypeNodeName)
                {
                    reader.ReadEndElement();
                }

                types.ExceptWith(serializerExtraTypes);
                serializer = GetXmlSerializerLegacy(types);
            }
            else serializer = GetXmlSerializer(types);

            using (var workflowReader = new StringReader(workflowMarkup))
            {
                var descriptor = (ExpressionBuilderGraphDescriptor)serializer.Deserialize(workflowReader);
                workflow.AddDescriptor(descriptor);
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            var types = new HashSet<Type>(GetExtensionTypes(workflow));
            foreach (var type in types)
            {
                if (!type.IsPublic)
                {
                    throw new InvalidOperationException(Resources.Exception_SerializingNonPublicType);
                }

                if (type.BaseType == typeof(UnknownTypeBuilder))
                {
                    throw new InvalidOperationException(Resources.Exception_SerializingUnknownTypeBuilder);
                }
            }

            Dictionary<string, GenericTypeCode> genericTypes;
            var serializer = GetXmlSerializer(types, out genericTypes);
            writer = new XmlExtensionWriter(writer, genericTypes);

            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            writer.WriteAttributeString(VersionAttributeName, versionInfo.ProductVersion);

            var description = Description;
            if (!string.IsNullOrEmpty(description))
            {
                writer.WriteElementString(DescriptionElementName, description);
            }

            var serializerNamespaces = GetXmlSerializerNamespaces(types);
            serializer.Serialize(writer, workflow.ToDescriptor(), serializerNamespaces);
        }

        #endregion

        #region SerializerFactory

        static class SerializerFactory
        {
            internal static readonly XmlSerializer instance = new XmlSerializer(typeof(WorkflowBuilder));
        }

        #endregion

        #region XmlSerializer Cache

        static HashSet<Type> serializerTypes;
        static XmlSerializer serializerCache;
        static Dictionary<string, GenericTypeCode> genericTypeCache;
        static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        static readonly object cacheLock = new object();
        static readonly string SystemNamespace = GetClrNamespace(typeof(object));
        static readonly string SystemCollectionsGenericNamespace = GetClrNamespace(typeof(IEnumerable<>));
        static readonly string BonsaiReactiveNamespace = GetClrNamespace(typeof(Bonsai.Reactive.Zip));
        static readonly Type[] serializerExtraTypes = GetDefaultSerializerTypes().ToArray();

        static IEnumerable<Type> GetDefaultSerializerTypes()
        {
            var builderType = typeof(ExpressionBuilder);
            return builderType.Assembly.GetTypes().Where(type =>
                !type.IsGenericType && !type.IsAbstract &&
                type.Namespace == builderType.Namespace &&
                Attribute.IsDefined(type, typeof(XmlTypeAttribute), false) &&
                !Attribute.IsDefined(type, typeof(ObsoleteAttribute), false));
        }

        static string GetClrNamespace(Type type)
        {
            if (type.Assembly == typeof(WorkflowBuilder).Assembly &&
                type.Namespace == typeof(ExpressionBuilder).Namespace)
            {
                return Constants.XmlNamespace;
            }

            var assemblyName = type.Assembly.GetName().Name.Replace(DynamicAssemblyPrefix, string.Empty);
            return string.Format("clr-namespace:{0};assembly={1}", type.Namespace, assemblyName);
        }

        static void GetXmlNamespaces(Type type, HashSet<string> xmlNamespaces)
        {
            xmlNamespaces.Add(GetClrNamespace(type));
            if (type.IsGenericType)
            {
                var typeArguments = type.GetGenericArguments();
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    GetXmlNamespaces(typeArguments[i], xmlNamespaces);
                }
            }
        }

        static XmlSerializerNamespaces GetXmlSerializerNamespaces(HashSet<Type> types)
        {
            int namespaceIndex = 1;
            var xmlNamespaces = new HashSet<string>();
            foreach (var type in types) GetXmlNamespaces(type, xmlNamespaces);

            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add("xsd", XmlSchema.Namespace);
            serializerNamespaces.Add("xsi", XmlSchema.InstanceNamespace);
            foreach (var xmlNamespace in xmlNamespaces)
            {
                if (xmlNamespace == Constants.XmlNamespace) continue;
                if (xmlNamespace == SystemNamespace) serializerNamespaces.Add("sys", SystemNamespace);
                else if (xmlNamespace == SystemCollectionsGenericNamespace) serializerNamespaces.Add("scg", SystemCollectionsGenericNamespace);
                else if (xmlNamespace == BonsaiReactiveNamespace) serializerNamespaces.Add("rx", BonsaiReactiveNamespace);
                else serializerNamespaces.Add("q" + namespaceIndex, xmlNamespace);
                namespaceIndex++;
            }

            return serializerNamespaces;
        }

        static XmlSerializer GetXmlSerializerLegacy(HashSet<Type> serializerTypes)
        {
            var overrides = new XmlAttributeOverrides();
            foreach (var type in serializerTypes)
            {
                var obsolete = Attribute.IsDefined(type, typeof(ObsoleteAttribute), false);
                var xmlTypeDefined = Attribute.IsDefined(type, typeof(XmlTypeAttribute), false);
                if (xmlTypeDefined && !obsolete) continue;

                var attributes = new XmlAttributes();
                if (obsolete && xmlTypeDefined)
                {
                    var xmlType = (XmlTypeAttribute)Attribute.GetCustomAttribute(type, typeof(XmlTypeAttribute));
                    attributes.XmlType = xmlType;
                }
                else attributes.XmlType = new XmlTypeAttribute { Namespace = GetClrNamespace(type) };
                overrides.Add(type, attributes);
            }

            var extraTypes = serializerTypes.Concat(serializerExtraTypes).ToArray();
            overrides.Add(typeof(SourceBuilder), new XmlAttributes { XmlType = new XmlTypeAttribute("Source") { Namespace = Constants.XmlNamespace } });
            overrides.Add(typeof(WindowWorkflowBuilder), new XmlAttributes { XmlType = new XmlTypeAttribute("WindowWorkflow") { Namespace = Constants.XmlNamespace } });
            var rootAttribute = new XmlRootAttribute(WorkflowNodeName) { Namespace = Constants.XmlNamespace };
            return new XmlSerializer(typeof(ExpressionBuilderGraphDescriptor), overrides, extraTypes, rootAttribute, null);
        }

        static XmlSerializer GetXmlSerializer(HashSet<Type> types)
        {
            Dictionary<string, GenericTypeCode> genericTypes;
            return GetXmlSerializer(types, out genericTypes);
        }

        static XmlSerializer GetXmlSerializer(HashSet<Type> types, out Dictionary<string, GenericTypeCode> genericTypes)
        {
            lock (cacheLock)
            {
                if (serializerCache == null || !types.IsSubsetOf(serializerTypes))
                {
                    if (serializerTypes == null) serializerTypes = types;
                    else serializerTypes.UnionWith(types);

                    genericTypeCache = new Dictionary<string, GenericTypeCode>();
                    XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                    foreach (var type in serializerTypes)
                    {
                        var xmlTypeDefined = Attribute.IsDefined(type, typeof(XmlTypeAttribute), false);
                        var attributes = new XmlAttributes();
                        attributes.XmlType = xmlTypeDefined
                            ? (XmlTypeAttribute)Attribute.GetCustomAttribute(type, typeof(XmlTypeAttribute))
                            : new XmlTypeAttribute();
                        
                        if (type.IsGenericType)
                        {
                            var typeRef = new CodeTypeReference(type);
                            var typeName = codeProvider.GetTypeOutput(typeRef);
                            genericTypeCache.Add(typeName, GenericTypeCode.FromType(type));
                            attributes.XmlType.TypeName = typeName;
                        }
                        else attributes.XmlType.Namespace = GetClrNamespace(type);
                        overrides.Add(type, attributes);
                    }

                    var extraTypes = serializerTypes.Concat(serializerExtraTypes).ToArray();
                    var rootAttribute = new XmlRootAttribute(WorkflowNodeName) { Namespace = Constants.XmlNamespace };
                    serializerCache = new XmlSerializer(typeof(ExpressionBuilderGraphDescriptor), overrides, extraTypes, rootAttribute, null);
                }

                genericTypes = genericTypeCache;
                return serializerCache;
            }
        }

        static IEnumerable<object> GetWorkflowElements(ExpressionBuilder builder)
        {
            yield return builder;
            var element = ExpressionBuilder.GetWorkflowElement(builder);
            if (element != builder) yield return element;

            var workflowBuilder = element as WorkflowExpressionBuilder;
            if (workflowBuilder != null)
            {
                foreach (var nestedElement in workflowBuilder.Workflow.SelectMany(node => GetWorkflowElements(node.Value)))
                {
                    yield return nestedElement;
                }
            }

            var serializableElement = element as ISerializableElement;
            if (serializableElement != null && (element = serializableElement.Element) != null)
            {
                yield return element;
            }
        }

        static IEnumerable<Type> GetExtensionTypes(ExpressionBuilderGraph workflow)
        {
            return workflow.SelectMany(node => GetWorkflowElements(node.Value))
                .Select(element => element.GetType())
                .Except(serializerExtraTypes);
        }

        #endregion

        #region UnknownTypeResolver

        static readonly UnknownTypeResolver TypeResolver = new UnknownTypeResolver();
        static readonly object typeResolverLock = new object();

        class UnknownTypeResolver
        {
            readonly Dictionary<string, AssemblyBuilder> dynamicAssemblies = new Dictionary<string, AssemblyBuilder>();
            readonly Dictionary<string, ModuleBuilder> dynamicModules = new Dictionary<string, ModuleBuilder>();
            readonly Dictionary<string, Type> dynamicTypes = new Dictionary<string, Type>();

            AssemblyBuilder GetDynamicAssembly(string name)
            {
                AssemblyBuilder assemblyBuilder;
                if (!dynamicAssemblies.TryGetValue(name, out assemblyBuilder))
                {
                    var assemblyName = new AssemblyName(DynamicAssemblyPrefix + name);
                    assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                    dynamicAssemblies.Add(name, assemblyBuilder);
                }
                return assemblyBuilder;
            }

            public Assembly ResolveAssembly(AssemblyName assemblyName)
            {
                try { return Assembly.Load(assemblyName); }
                catch (SystemException ex)
                {
                    if (ex is IOException || ex is BadImageFormatException)
                    {
                        return GetDynamicAssembly(assemblyName.FullName);
                    }

                    throw;
                }
            }

            public Type ResolveType(Assembly assembly, string typeName, bool ignoreCase)
            {
                Type type;
                string message = Resources.Exception_UnknownTypeBuilder;
                try { type = assembly.GetType(typeName, false, ignoreCase); }
                catch (SystemException ex)
                {
                    if (ex is IOException || ex is BadImageFormatException || ex is TypeLoadException)
                    {
                        message = string.Join(" ", Resources.Exception_TypeLoadException, ex.Message);
                        type = null;
                    }
                    else throw;
                }

                if (type == null)
                {
                    var assemblyBuilder = assembly as AssemblyBuilder;
                    if (assemblyBuilder == null)
                    {
                        assemblyBuilder = GetDynamicAssembly(assembly.FullName);
                    }

                    if (!dynamicTypes.TryGetValue(typeName, out type))
                    {
                        ModuleBuilder moduleBuilder;
                        if (!dynamicModules.TryGetValue(assembly.FullName, out moduleBuilder))
                        {
                            moduleBuilder = assemblyBuilder.DefineDynamicModule(assembly.FullName);
                            dynamicModules.Add(assembly.FullName, moduleBuilder);
                        }

                        var typeBuilder = moduleBuilder.DefineType(
                            typeName,
                            TypeAttributes.Public | TypeAttributes.Class,
                            typeof(UnknownTypeBuilder));
                        var errorMessage = string.Format(message, typeBuilder.FullName);
                        var descriptionAttributeConstructor = typeof(DescriptionAttribute).GetConstructor(new[] { typeof(string) });
                        var descriptionAttributeBuilder = new CustomAttributeBuilder(descriptionAttributeConstructor, new[] { errorMessage });
                        var obsoleteAttributeConstructor = typeof(ObsoleteAttribute).GetConstructor(Type.EmptyTypes);
                        var obsoleteAttributeBuilder = new CustomAttributeBuilder(obsoleteAttributeConstructor, new object[0]);
                        typeBuilder.SetCustomAttribute(descriptionAttributeBuilder);
                        typeBuilder.SetCustomAttribute(obsoleteAttributeBuilder);
                        type = typeBuilder.CreateType();
                        dynamicTypes.Add(typeName, type);
                    }
                }

                return type;
            }
        }

        #endregion

        #region ConvertDescriptorMarkup

        static readonly Lazy<XslCompiledTransform> descriptorXslt = new Lazy<XslCompiledTransform>(() =>
        {
            const string XsltMarkup = @"
<xsl:stylesheet version=""1.0""
                xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
                xmlns:bonsai=""https://horizongir.org/bonsai""
                exclude-result-prefixes=""bonsai"">
  <xsl:output method=""xml"" indent=""yes""/>
  <xsl:variable name=""uri"" select=""'https://bonsai-rx.org/2018/workflow'""/>
  <xsl:template match=""@* | node()"">
    <xsl:copy>
      <xsl:apply-templates select=""@* | node()""/>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match=""bonsai:*"">
    <xsl:element name=""{local-name()}"" namespace=""{$uri}"">
      <xsl:copy-of select=""namespace::*[local-name() != '']""/>
      <xsl:apply-templates select=""@* | node()""/>
    </xsl:element>
  </xsl:template>

  <xsl:template match=""@bonsai:*"">
    <xsl:attribute name=""{local-name()}"" namespace=""{$uri}"">
      <xsl:value-of select="".""/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match=""bonsai:PropertyMappings/bonsai:Property"">
    <xsl:element name=""Property"" namespace=""{$uri}"">
      <xsl:attribute name=""Name"">
        <xsl:value-of select=""@name""/>
      </xsl:attribute>
      <xsl:if test=""@selector"">
        <xsl:attribute name=""Selector"">
          <xsl:value-of select=""@selector""/>
        </xsl:attribute>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match=""bonsai:Workflow/bonsai:Edges/bonsai:Edge"">
    <xsl:element name=""Edge"" namespace=""{$uri}"">
      <xsl:attribute name=""From"">
        <xsl:value-of select=""bonsai:From""/>
      </xsl:attribute>
      <xsl:attribute name=""To"">
        <xsl:value-of select=""bonsai:To""/>
      </xsl:attribute>
      <xsl:attribute name=""Label"">
        <xsl:value-of select=""bonsai:Label""/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>";
            var xslt = new XslCompiledTransform();
            using (var reader = XmlReader.Create(new StringReader(XsltMarkup)))
            {
                xslt.Load(reader);
            }
            return xslt;
        });

        static string ConvertDescriptorMarkup(string workflowMarkup)
        {
            using (var reader = new StringReader(workflowMarkup))
            using (var xmlReader = XmlReader.Create(reader))
            {
                var xslt = descriptorXslt.Value;
                using (var writer = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(writer, xslt.OutputSettings))
                {
                    xslt.Transform(xmlReader, xmlWriter);
                    return writer.ToString();
                }
            }
        }

        #endregion

        #region ReadXmlExtensions

        static string Split(string value, char separator, out string prefix)
        {
            var index = value.IndexOf(separator);
            return Split(value, index, 1, out prefix);
        }

        static string Split(string value, string separator, out string prefix)
        {
            var index = value.IndexOf(separator);
            return Split(value, index, separator.Length, out prefix);
        }

        static string Split(string value, int index, int offset, out string prefix)
        {
            if (index >= 0)
            {
                prefix = value.Substring(0, index);
                return value.Substring(index + offset);
            }
            else
            {
                prefix = string.Empty;
                return value;
            }
        }

        struct GenericTypeToken
        {
            public string Token;
            public List<Type> TypeArguments;
        }

        static Type[] ParseTypeArguments(XmlReader reader, string value)
        {
            var i = 0;
            var hasNext = false;
            var builder = new StringBuilder(value.Length);
            var typeArguments = new List<Type>();
            var stack = new Stack<GenericTypeToken>();
            do
            {
                hasNext = i < value.Length;
                var c = hasNext ? value[i++] : ',';
                switch (c)
                {
                    case '(':
                        GenericTypeToken genericType;
                        genericType.Token = builder.ToString();
                        genericType.TypeArguments = typeArguments;
                        typeArguments = new List<Type>();
                        stack.Push(genericType);
                        builder.Clear();
                        break;
                    case ',':
                    case ')':
                        if (builder.Length > 0)
                        {
                            var token = builder.ToString();
                            var type = LookupType(reader, token);
                            typeArguments.Add(type);
                            builder.Clear();
                        }

                        if (c == ')')
                        {
                            var baseType = stack.Pop();
                            var type = LookupType(reader, baseType.Token, typeArguments.ToArray());
                            typeArguments = baseType.TypeArguments;
                            typeArguments.Add(type);
                        }
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
            while (hasNext);
            return typeArguments.ToArray();
        }

        static Type LookupType(XmlReader reader, string name, params Type[] typeArguments)
        {
            string prefix, ns;
            name = ResolveTypeName(reader, name, out prefix, out ns);
            return LookupType(name, ns, typeArguments);
        }

        static Type LookupType(string name, string ns, params Type[] typeArguments)
        {
            if (typeArguments != null && typeArguments.Length > 0)
            {
                name = name + '`' + typeArguments.Length;
            }

            var assembly = Split(ns, ";assembly=", out ns);
            var typeName = string.IsNullOrEmpty(ns)
                ? name + "," + assembly
                : ns + "." + name + "," + assembly;
            return LookupType(typeName, typeArguments);
        }

        static Type LookupType(string typeName, params Type[] typeArguments)
        {
            var type = default(Type);
            try { type = Type.GetType(typeName, false); }
            catch (IOException) { }
            catch (BadImageFormatException) { }
            catch (TypeLoadException) { }
            if (type == null)
            {
                lock (typeResolverLock)
                {
                    type = Type.GetType(typeName, TypeResolver.ResolveAssembly, TypeResolver.ResolveType, true);
                }
            }
            return type.IsGenericTypeDefinition ? type.MakeGenericType(typeArguments) : type;
        }

        static string ResolveTypeName(XmlReader reader, string value, out string prefix, out string ns)
        {
            var name = Split(value, ':', out prefix);
            ns = reader.LookupNamespace(prefix);
            if (ns == Constants.XmlNamespace) ns = "Bonsai.Expressions;assembly=Bonsai.Core";
            else
            {
                ns = Split(ns, ':', out prefix);
                if (prefix != "clr-namespace")
                {
                    throw new InvalidOperationException(string.Format(Resources.Exception_InvalidTypeNamespace, value));
                }
            }
            return name;
        }

        static Type ResolveXmlExtension(XmlReader reader, string value, string typeArguments)
        {
            string prefix, ns;
            var name = ResolveTypeName(reader, value, out prefix, out ns);
            if (prefix == "clr-namespace" || !string.IsNullOrEmpty(typeArguments))
            {
                Type[] genericArguments = null;
                if (!string.IsNullOrEmpty(typeArguments))
                {
                    genericArguments = ParseTypeArguments(reader, typeArguments);
                }

                return LookupType(name, ns, genericArguments);
            }

            return null;
        }

        static void WriteXmlAttributes(XmlReader reader, XmlWriter writer, bool lookupTypes, HashSet<Type> types)
        {
            do
            {
                if (!reader.IsDefault && (!lookupTypes || reader.LocalName != TypeArgumentsAttributeName))
                {
                    var ns = reader.NamespaceURI;
                    writer.WriteStartAttribute(reader.Prefix, reader.LocalName, ns);
                    while (reader.ReadAttributeValue())
                    {
                        if (reader.NodeType == XmlNodeType.EntityReference)
                        {
                            writer.WriteEntityRef(reader.Name);
                        }
                        else
                        {
                            var value = reader.Value;
                            // ensure xsi:type attributes are resolved only for workflow element types
                            if (ns == XmlSchema.InstanceNamespace && lookupTypes)
                            {
                                var typeArguments = reader.GetAttribute(TypeArgumentsAttributeName);
                                var type = ResolveXmlExtension(reader, value, typeArguments);
                                if (type != null)
                                {
                                    types.Add(type);
                                    if (!string.IsNullOrEmpty(typeArguments))
                                    {
                                        var typeRef = new CodeTypeReference(type);
                                        var typeName = codeProvider.GetTypeOutput(typeRef);
                                        value = XmlConvert.EncodeName(typeName);
                                    }
                                }
                            }

                            writer.WriteString(value);
                        }
                    }
                    writer.WriteEndAttribute();
                }
            }
            while (reader.MoveToNextAttribute());
        }

        static string ReadXmlExtensions(XmlReader reader, HashSet<Type> types)
        {
            const int ChunkBufferSize = 1024;
            char[] chunkBuffer = null;

            var canReadChunk = reader.CanReadValueChunk;
            var depth = reader.NodeType == XmlNodeType.None ? -1 : reader.Depth;
            var sw = new StringWriter(CultureInfo.InvariantCulture);
            using (var writer = XmlWriter.Create(sw))
            {
                do
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            var elementNamespace = reader.NamespaceURI;
                            writer.WriteStartElement(reader.Prefix, reader.LocalName, elementNamespace);
                            if (reader.MoveToFirstAttribute())
                            {
                                var lookupTypes = elementNamespace == Constants.XmlNamespace;
                                WriteXmlAttributes(reader, writer, lookupTypes, types);
                                reader.MoveToElement();
                            }

                            if (reader.IsEmptyElement)
                            {
                                writer.WriteEndElement();
                            }
                            break;
                        case XmlNodeType.Text:
                            if (canReadChunk)
                            {
                                int chunkSize;
                                if (chunkBuffer == null) chunkBuffer = new char[ChunkBufferSize];
                                while ((chunkSize = reader.ReadValueChunk(chunkBuffer, 0, ChunkBufferSize)) > 0)
                                {
                                    writer.WriteChars(chunkBuffer, 0, chunkSize);
                                }
                            }
                            else writer.WriteString(reader.Value);
                            break;
                        case XmlNodeType.CDATA:
                            writer.WriteCData(reader.Value);
                            break;
                        case XmlNodeType.Comment:
                            writer.WriteComment(reader.Value);
                            break;
                        case XmlNodeType.EndElement:
                            writer.WriteFullEndElement();
                            break;
                        case XmlNodeType.EntityReference:
                            writer.WriteEntityRef(reader.Name);
                            break;
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            writer.WriteWhitespace(reader.Value);
                            break;
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                            writer.WriteProcessingInstruction(reader.Name, reader.Value);
                            break;
                    }
                }
                while (reader.Read() && (depth < reader.Depth || (depth == reader.Depth && reader.NodeType == XmlNodeType.EndElement)));
            }
            return sw.ToString();
        }

        #endregion

        #region XmlExtensionWriter

        class GenericTypeCode
        {
            public string Name;
            public string Namespace;
            public GenericTypeCode[] TypeArguments;
            static readonly GenericTypeCode[] EmptyTypes = new GenericTypeCode[0];

            public static GenericTypeCode FromType(Type type)
            {
                var code = new GenericTypeCode();
                code.Name = type.Name;
                code.Namespace = GetClrNamespace(type);
                if (type.IsGenericType)
                {
                    code.Name = code.Name.Substring(0, code.Name.LastIndexOf('`'));
                    code.TypeArguments = Array.ConvertAll(type.GetGenericArguments(), FromType);
                }
                else code.TypeArguments = EmptyTypes;
                return code;
            }
        }

        class XmlExtensionWriter : XmlWriter
        {
            bool xsiTypeAttribute;
            string xsiTypeArguments;
            readonly XmlWriter writer;
            readonly Dictionary<string, GenericTypeCode> genericTypes;

            public XmlExtensionWriter(XmlWriter writer, Dictionary<string, GenericTypeCode> genericTypes)
            {
                this.writer = writer;
                this.genericTypes = genericTypes;
            }

            public override XmlWriterSettings Settings
            {
                get { return writer.Settings; }
            }

            public override WriteState WriteState
            {
                get { return writer.WriteState; }
            }

            public override string XmlLang
            {
                get { return writer.XmlLang; }
            }

            public override XmlSpace XmlSpace
            {
                get { return writer.XmlSpace; }
            }

            public override void Flush()
            {
                writer.Flush();
            }

            public override string LookupPrefix(string ns)
            {
                return writer.LookupPrefix(ns);
            }

            public override void WriteBase64(byte[] buffer, int index, int count)
            {
                writer.WriteBase64(buffer, index, count);
            }

            public override void WriteBinHex(byte[] buffer, int index, int count)
            {
                writer.WriteBinHex(buffer, index, count);
            }

            public override void WriteCData(string text)
            {
                writer.WriteCData(text);
            }

            public override void WriteCharEntity(char ch)
            {
                writer.WriteCharEntity(ch);
            }

            public override void WriteChars(char[] buffer, int index, int count)
            {
                writer.WriteChars(buffer, index, count);
            }

            public override void WriteComment(string text)
            {
                writer.WriteComment(text);
            }

            public override void WriteDocType(string name, string pubid, string sysid, string subset)
            {
                writer.WriteDocType(name, pubid, sysid, subset);
            }

            public override void WriteEndAttribute()
            {
                writer.WriteEndAttribute();
                if (xsiTypeArguments != null)
                {
                    writer.WriteAttributeString(TypeArgumentsAttributeName, xsiTypeArguments);
                    xsiTypeArguments = null;
                }
            }

            public override void WriteEndDocument()
            {
                writer.WriteEndDocument();
            }

            public override void WriteEndElement()
            {
                writer.WriteEndElement();
            }

            public override void WriteEntityRef(string name)
            {
                writer.WriteEntityRef(name);
            }

            public override void WriteFullEndElement()
            {
                writer.WriteFullEndElement();
            }

            public override void WriteName(string name)
            {
                writer.WriteName(name);
            }

            public override void WriteNmToken(string name)
            {
                writer.WriteNmToken(name);
            }

            public override void WriteProcessingInstruction(string name, string text)
            {
                writer.WriteProcessingInstruction(name, text);
            }

            public override void WriteQualifiedName(string localName, string ns)
            {
                writer.WriteQualifiedName(localName, ns);
            }

            public override void WriteRaw(string data)
            {
                writer.WriteRaw(data);
            }

            public override void WriteRaw(char[] buffer, int index, int count)
            {
                writer.WriteRaw(buffer, index, count);
            }

            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                xsiTypeAttribute = ns == XmlSchema.InstanceNamespace && localName == "type";
                writer.WriteStartAttribute(prefix, localName, ns);
            }

            public override void WriteStartDocument(bool standalone)
            {
                writer.WriteStartDocument(standalone);
            }

            public override void WriteStartDocument()
            {
                writer.WriteStartDocument();
            }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                writer.WriteStartElement(prefix, localName, ns);
            }

            string EncodeGenericType(GenericTypeCode type)
            {
                var prefix = writer.LookupPrefix(type.Namespace);
                return string.IsNullOrEmpty(prefix) ? type.Name : prefix + ":" + type.Name;
            }

            string EncodeGenericTypeArguments(GenericTypeCode[] typeArguments)
            {
                var builder = new StringBuilder();
                EncodeGenericTypeArguments(builder, typeArguments);
                return builder.ToString();
            }

            void EncodeGenericTypeArguments(StringBuilder builder, GenericTypeCode[] typeArguments)
            {
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    if (i > 0) builder.Append(',');
                    var typeArgument = typeArguments[i];
                    builder.Append(EncodeGenericType(typeArgument));
                    if (typeArgument.TypeArguments.Length > 0)
                    {
                        builder.Append('(');
                        EncodeGenericTypeArguments(builder, typeArgument.TypeArguments);
                        builder.Append(')');
                    }
                }
            }

            public override void WriteString(string text)
            {
                if (writer.WriteState == WriteState.Attribute && xsiTypeAttribute)
                {
                    GenericTypeCode type;
                    var typeName = XmlConvert.DecodeName(text);
                    if (genericTypes.TryGetValue(typeName, out type))
                    {
                        text = EncodeGenericType(type);
                        if (type.TypeArguments.Length > 0)
                        {
                            xsiTypeArguments = EncodeGenericTypeArguments(type.TypeArguments);
                        }
                    }
                    xsiTypeAttribute = false;
                }

                writer.WriteString(text);
            }

            public override void WriteSurrogateCharEntity(char lowChar, char highChar)
            {
                writer.WriteSurrogateCharEntity(lowChar, highChar);
            }

            public override void WriteWhitespace(string ws)
            {
                writer.WriteWhitespace(ws);
            }
        }

        #endregion
    }
}
