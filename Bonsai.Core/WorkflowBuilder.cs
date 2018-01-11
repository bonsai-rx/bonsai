﻿using System;
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

            var workflowMarkup = string.Empty;
            if (reader.IsStartElement(WorkflowNodeName))
            {
                workflowMarkup = reader.ReadOuterXml();
            }

            reader.ReadToFollowing(ExtensionTypeNodeName);
            reader.ReadStartElement();
            var types = new HashSet<Type>();
            while (reader.ReadToNextSibling(TypeNodeName))
            {
                var typeName = reader.ReadElementString();
                var type = Type.GetType(typeName, false);
                if (type == null)
                {
                    lock (typeResolverLock)
                    {
                        type = Type.GetType(typeName, TypeResolver.ResolveAssembly, TypeResolver.ResolveType, true);
                    }
                }

                types.Add(type);
            }

            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == ExtensionTypeNodeName)
            {
                reader.ReadEndElement();
            }

            var serializer = GetXmlSerializer(types);
            using (var workflowReader = new StringReader(workflowMarkup))
            {
                var descriptor = (ExpressionBuilderGraphDescriptor)serializer.Deserialize(workflowReader);
                workflow.AddDescriptor(descriptor);
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            writer.WriteAttributeString(VersionAttributeName, versionInfo.ProductVersion);

            var description = Description;
            if (!string.IsNullOrEmpty(description))
            {
                writer.WriteElementString(DescriptionElementName, description);
            }

            var types = new HashSet<Type>(GetExtensionTypes(workflow));
            var serializer = GetXmlSerializer(types);
            var serializerNamespaces = GetXmlSerializerNamespaces(types);
            serializer.Serialize(writer, workflow.ToDescriptor(), serializerNamespaces);

            writer.WriteStartElement(ExtensionTypeNodeName);
            foreach (var type in types)
            {
                if (type.BaseType == typeof(UnknownTypeBuilder))
                {
                    throw new InvalidOperationException(Resources.Exception_SerializingUnknownTypeBuilder);
                }

                writer.WriteElementString(TypeNodeName, type.AssemblyQualifiedName.Replace(DynamicAssemblyPrefix, string.Empty));
            }
            writer.WriteEndElement();
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
        static readonly object cacheLock = new object();

        static IEnumerable<Type> GetDefaultSerializerTypes()
        {
            return Attribute.GetCustomAttributes(typeof(ExpressionBuilder), typeof(XmlIncludeAttribute), false)
                            .Select(attribute => ((XmlIncludeAttribute)attribute).Type);
        }

        static string GetXmlNamespace(Type type)
        {
            var xmlTypeAttribute = (XmlTypeAttribute)Attribute.GetCustomAttribute(type, typeof(XmlTypeAttribute), false);
            if (xmlTypeAttribute != null) return xmlTypeAttribute.Namespace;
            return GetClrNamespace(type);
        }

        static string GetClrNamespace(Type type)
        {
            var assemblyName = type.Assembly.GetName().Name.Replace(DynamicAssemblyPrefix, string.Empty);
            return string.Format("clr-namespace:{0};assembly={1}", type.Namespace, assemblyName);
        }

        static XmlSerializerNamespaces GetXmlSerializerNamespaces(HashSet<Type> types)
        {
            int namespaceIndex = 1;
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add("xsd", XmlSchema.Namespace);
            serializerNamespaces.Add("xsi", XmlSchema.InstanceNamespace);
            foreach (var xmlNamespace in (from type in types
                                          let xmlNamespace = GetXmlNamespace(type)
                                          where xmlNamespace != Constants.XmlNamespace
                                          select xmlNamespace)
                                         .Distinct())
            {
                serializerNamespaces.Add("q" + namespaceIndex, xmlNamespace);
                namespaceIndex++;
            }

            return serializerNamespaces;
        }

        static XmlSerializer GetXmlSerializer(HashSet<Type> types)
        {
            lock (cacheLock)
            {
                if (serializerCache == null || !types.IsSubsetOf(serializerTypes))
                {
                    if (serializerTypes == null) serializerTypes = types;
                    else serializerTypes.UnionWith(types);

                    XmlAttributeOverrides overrides = new XmlAttributeOverrides();
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

                    overrides.Add(typeof(SourceBuilder), new XmlAttributes { XmlType = new XmlTypeAttribute("Source") { Namespace = Constants.XmlNamespace } });
                    var rootAttribute = new XmlRootAttribute(WorkflowNodeName) { Namespace = Constants.XmlNamespace };
                    serializerCache = new XmlSerializer(typeof(ExpressionBuilderGraphDescriptor), overrides, serializerTypes.ToArray(), rootAttribute, null);
                }
            }

            return serializerCache;
        }

        static IEnumerable<object> GetWorkflowElements(ExpressionBuilder builder)
        {
            yield return builder;
            var element = ExpressionBuilder.GetWorkflowElement(builder);
            if (element != builder) yield return element;

            var binaryOperator = builder as BinaryOperatorBuilder;
            if (binaryOperator != null && binaryOperator.Operand != null) yield return binaryOperator.Operand;
        }

        static IEnumerable<Type> GetWorkflowElementTypes(ExpressionBuilder builder)
        {
            var workflowExpressionBuilder = ExpressionBuilder.GetWorkflowElement(builder) as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                return GetExtensionTypes(workflowExpressionBuilder.Workflow);
            }
            else return GetWorkflowElements(builder).Select(element => element.GetType());
        }

        static IEnumerable<Type> GetExtensionTypes(ExpressionBuilderGraph workflow)
        {
            return workflow.SelectMany(node => GetWorkflowElementTypes(node.Value)
                .Concat(Enumerable.Repeat(node.Value.GetType(), 1)))
                .Except(GetDefaultSerializerTypes())
                .Where(type => type.IsPublic);
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
                catch (IOException)
                {
                    return GetDynamicAssembly(assemblyName.FullName);
                }
            }

            public Type ResolveType(Assembly assembly, string typeName, bool ignoreCase)
            {
                var type = assembly.GetType(typeName, false, ignoreCase);
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
                        var obsoleteAttributeConstructor = typeof(ObsoleteAttribute).GetConstructor(Type.EmptyTypes);
                        var obsoleteAttributeBuilder = new CustomAttributeBuilder(obsoleteAttributeConstructor, new object[0]);
                        typeBuilder.SetCustomAttribute(obsoleteAttributeBuilder);
                        type = typeBuilder.CreateType();
                        dynamicTypes.Add(typeName, type);
                    }
                }

                return type;
            }
        }

        #endregion
    }
}
