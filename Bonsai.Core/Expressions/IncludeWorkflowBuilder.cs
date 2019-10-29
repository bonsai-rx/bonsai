using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using SystemPath = System.IO.Path;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that uses an encapsulated workflow stored
    /// externally to generate its output.
    /// </summary>
    [DefaultProperty("Path")]
    [WorkflowElementCategory(ElementCategory.Workflow)]
    [XmlType("IncludeWorkflow", Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(IncludeWorkflowTypeDescriptionProvider))]
    public sealed class IncludeWorkflowBuilder : VariableArgumentExpressionBuilder, IGroupWorkflowBuilder, INamedElement, IRequireBuildContext
    {
        const char AssemblySeparator = ':';
        static readonly XElement[] EmptyProperties = new XElement[0];
        static readonly XmlSerializerNamespaces DefaultSerializerNamespaces = GetXmlSerializerNamespaces();
        XElement[] xmlProperties;

        IBuildContext buildContext;
        ExpressionBuilderGraph workflow;
        readonly bool inspectWorkflow;
        DateTime writeTime;
        string description;
        string path;
        string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludeWorkflowBuilder"/> class.
        /// </summary>
        public IncludeWorkflowBuilder()
            : base(minArguments: 0, maxArguments: 1)
        {
        }

        internal IncludeWorkflowBuilder(IncludeWorkflowBuilder builder, bool inspect)
            : base(builder.ArgumentRange.LowerBound, builder.ArgumentRange.UpperBound)
        {
            inspectWorkflow = inspect;
            workflow = builder.workflow;
            if (workflow != null && inspect != builder.inspectWorkflow)
            {
                workflow = inspect ? workflow.ToInspectableGraph() : workflow.FromInspectableGraph();
            }

            writeTime = builder.writeTime;
            name = builder.name;
            path = builder.path;
            description = builder.description;
            xmlProperties = builder.xmlProperties;
        }

        /// <summary>
        /// Gets the expression builder workflow that will be used to generate the
        /// output expression tree.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

        /// <summary>
        /// Gets the name of the included workflow.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets a description for the included workflow.
        /// </summary>
        [Browsable(false)]
        public string Description
        {
            get { return description; }
        }

        IBuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set
            {
                buildContext = value;
                if (buildContext != null)
                {
                    EnsureWorkflow();
                    xmlProperties = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the path of the workflow to include.
        /// </summary>
        [XmlAttribute]
        [Category("Design")]
        [Externalizable(false)]
        [FileNameFilter("Bonsai Files (*.bonsai)|*.bonsai")]
        [Description("The path of the workflow to include.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path
        {
            get { return path; }
            set
            {
                path = GetWorkflowPath(value);
                writeTime = DateTime.MinValue;
                name = GetDisplayName(path);
            }
        }

        /// <summary>
        /// Gets or sets the XML representation of externalized property values from the included workflow.
        /// </summary>
        [XmlAnyElement]
        [Browsable(false)]
        public XElement[] PropertiesXml
        {
            get
            {
                if (xmlProperties != null) return xmlProperties;
                else if (workflow != null)
                {
                    return GetXmlProperties();
                }
                else return EmptyProperties;
            }
            set { xmlProperties = value; }
        }

        internal XElement[] InternalXmlProperties
        {
            get { return xmlProperties; }
        }

        static XmlSerializerNamespaces GetXmlSerializerNamespaces()
        {
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add("xsi", XmlSchema.InstanceNamespace);
            return serializerNamespaces;
        }

        XElement[] GetXmlProperties()
        {
            var properties = TypeDescriptor.GetProperties(this);
            return GetXmlSerializableProperties(properties).Select(SerializeProperty).ToArray();
        }

        void SetXmlProperties(XElement[] xmlProperties)
        {
            var properties = TypeDescriptor.GetProperties(this);
            var serializableProperties = GetXmlSerializableProperties(properties).ToDictionary(property => property.Name);
            for (int i = 0; i < xmlProperties.Length; i++)
            {
                ExternalizedPropertyDescriptor property;
                if (serializableProperties.TryGetValue(xmlProperties[i].Name.LocalName, out property))
                {
                    if (xmlProperties[i].NodeType == XmlNodeType.Text)
                    {
                        var value = xmlProperties[i].Value;
                        property = (ExternalizedPropertyDescriptor)properties[property.Name];
                        property.SetValue(this, property.Converter.ConvertFromInvariantString(value));
                    }
                    else DeserializeProperty(xmlProperties[i], property);
                }
            }
        }

        IEnumerable<ExternalizedPropertyDescriptor> GetXmlSerializableProperties(PropertyDescriptorCollection properties)
        {
            return from property in properties.Cast<PropertyDescriptor>()
                   let externalizedProperty = EnsureXmlSerializable(property as ExternalizedPropertyDescriptor)
                   where externalizedProperty != null && (!externalizedProperty.IsReadOnly || ExpressionHelper.IsCollectionType(externalizedProperty.PropertyType))
                   select externalizedProperty;
        }

        ExternalizedPropertyDescriptor EnsureXmlSerializable(ExternalizedPropertyDescriptor descriptor)
        {
            if (descriptor == null) return null;
            var xmlIgnore = descriptor.Attributes[typeof(XmlIgnoreAttribute)];
            if (xmlIgnore != null)
            {
                var converted = true;
                var serializableDescriptor = descriptor.Convert(externalizedProperty =>
                {
                    var proxyDescriptor = (from property in TypeDescriptor.GetProperties(externalizedProperty.ComponentType).Cast<PropertyDescriptor>()
                                           let xmlElement = (XmlElementAttribute)property.Attributes[typeof(XmlElementAttribute)]
                                           where xmlElement != null && xmlElement.ElementName == externalizedProperty.Name
                                           select property)
                                           .FirstOrDefault();
                    converted &= proxyDescriptor != null;
                    return proxyDescriptor;
                });
                return converted ? serializableDescriptor : null;
            }

            return descriptor;
        }

        void DeserializeProperty(XElement element, PropertyDescriptor property)
        {
            if (property.PropertyType == typeof(XElement))
            {
                property.SetValue(this, element);
                return;
            }

            var serializer = PropertySerializer.GetXmlSerializer(property.Name, property.PropertyType);
            using (var reader = element.CreateReader())
            {
                var value = serializer.Deserialize(reader);
                if (property.IsReadOnly)
                {
                    var collection = (IList)property.GetValue(this);
                    if (collection == null)
                    {
                        throw new InvalidOperationException("Collection reference not set to an instance of an object.");
                    }

                    collection.Clear();
                    var collectionElements = value as IEnumerable;
                    if (collectionElements != null)
                    {
                        foreach (var collectionElement in collectionElements)
                        {
                            collection.Add(collectionElement);
                        }
                    }
                }
                else property.SetValue(this, value);
            }
        }

        XElement SerializeProperty(PropertyDescriptor property)
        {
            var document = new XDocument();
            var serializer = PropertySerializer.GetXmlSerializer(property.Name, property.PropertyType);
            using (var writer = document.CreateWriter())
            {
                serializer.Serialize(writer, property.GetValue(this), DefaultSerializerNamespaces);
            }
            return document.Root;
        }

        static string GetWorkflowPath(string path)
        {
            if (SystemPath.GetExtension(path) == string.Empty)
            {
                return SystemPath.ChangeExtension(path, Constants.BonsaiExtension);
            }

            return path;
        }

        static bool IsEmbeddedResourcePath(string path)
        {
            var separatorIndex = path.IndexOf(AssemblySeparator);
            return separatorIndex >= 0 && !SystemPath.IsPathRooted(path);
        }

        static string GetDisplayName(string path)
        {
            var name = SystemPath.GetFileNameWithoutExtension(path);
            if (IsEmbeddedResourcePath(path))
            {
                var nameSeparator = name.LastIndexOf(ExpressionHelper.MemberSeparator);
                name = nameSeparator >= 0 ? name.Substring(nameSeparator + 1) : name;
            }

            return name;
        }

        static Stream GetWorkflowStream(string path, bool embeddedResource)
        {
            if (embeddedResource)
            {
                var nameElements = path.Split(new[] { AssemblySeparator }, 2);
                if (string.IsNullOrEmpty(nameElements[0]))
                {
                    throw new InvalidOperationException(
                        "The embedded resource path \"" + path +
                        "\" must be qualified with a valid assembly name.");
                }

                var assembly = System.Reflection.Assembly.Load(nameElements[0]);
                var resourceName = string.Join(ExpressionHelper.MemberSeparator, nameElements);
                var workflowStream = assembly.GetManifestResourceStream(resourceName);
                if (workflowStream == null)
                {
                    throw new InvalidOperationException(
                        "The specified embedded resource \"" + nameElements[1] +
                        "\" was not found in assembly \"" + nameElements[0] + "\"");
                }

                return workflowStream;
            }
            else
            {
                if (!File.Exists(path))
                {
                    throw new InvalidOperationException("The specified workflow could not be found.");
                }

                return File.OpenRead(path);
            }
        }

        void EnsureWorkflow()
        {
            var context = buildContext;
            while (context != null)
            {
                var includeContext = context as IncludeContext;
                if (includeContext != null && includeContext.Path == path)
                {
                    var message = string.Format("Included workflow '{0}' includes itself.", path);
                    throw new InvalidOperationException(message);
                }

                context = context.ParentContext;
            }

            if (string.IsNullOrEmpty(path))
            {
                workflow = null;
                description = null;
                SetArgumentRange(0, 1);
            }
            else
            {
                var embeddedResource = IsEmbeddedResourcePath(path);
                var lastWriteTime = embeddedResource ? DateTime.MaxValue : File.GetLastWriteTime(path);
                if (workflow == null || lastWriteTime > writeTime)
                {
                    var properties = workflow != null ? GetXmlProperties() : xmlProperties;
                    using (var stream = GetWorkflowStream(path, embeddedResource))
                    using (var reader = XmlReader.Create(stream))
                    {
                        reader.MoveToContent();
                        var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
                        var builder = (WorkflowBuilder)serializer.Deserialize(reader);
                        description = builder.Description;
                        workflow = builder.Workflow;
                        writeTime = lastWriteTime;
                    }

                    var parameterCount = workflow.GetNestedParameters().Count();
                    SetArgumentRange(0, parameterCount);
                    if (inspectWorkflow)
                    {
                        workflow = workflow.ToInspectableGraph();
                    }

                    if (properties != null)
                    {
                        SetXmlProperties(properties);
                    }
                }
            }
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            if (workflow == null)
            {
                if (string.IsNullOrEmpty(Path))
                {
                    return arguments.FirstOrDefault() ?? UndefinedExpression.Instance;
                }
                else throw new InvalidOperationException("The specified workflow could not be found.");
            }

            var includeContext = new IncludeContext(buildContext, path);
            return workflow.BuildNested(arguments, includeContext);
        }

        static class PropertySerializer
        {
            static readonly Dictionary<Tuple<string, Type>, XmlSerializer> serializerCache = new Dictionary<Tuple<string, Type>, XmlSerializer>();
            static readonly object cacheLock = new object();

            internal static XmlSerializer GetXmlSerializer(string name, Type type)
            {
                XmlSerializer serializer;
                var serializerKey = Tuple.Create(name, type);
                lock (cacheLock)
                {
                    if (!serializerCache.TryGetValue(serializerKey, out serializer))
                    {
                        var xmlRoot = new XmlRootAttribute(name) { Namespace = Constants.XmlNamespace };
                        serializer = new XmlSerializer(type, xmlRoot);
                        serializerCache.Add(serializerKey, serializer);
                    }
                }

                return serializer;
            }
        }

        class UndefinedExpression : Expression
        {
            internal static readonly UndefinedExpression Instance = new UndefinedExpression();

            private UndefinedExpression()
            {
            }

            public override ExpressionType NodeType
            {
                get { return ExpressionType.Extension; }
            }

            public override Type Type
            {
                get { throw new InvalidOperationException("Unable to evaluate included workflow expression."); }
            }
        }

        class IncludeWorkflowTypeDescriptionProvider : TypeDescriptionProvider
        {
            const string DefaultDescription = "Includes an encapsulated workflow from the specified path.";
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(IncludeWorkflowBuilder));

            public IncludeWorkflowTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                var builder = (IncludeWorkflowBuilder)instance;
                if (builder != null)
                {
                    var description = new IncludeWorkflowDescriptionAttribute(builder, DefaultDescription);
                    if (builder.Workflow == null) return new IncludeWorkflowXmlTypeDescriptor(builder, description);
                    else return new WorkflowTypeDescriptor(instance, description);
                }

                return base.GetExtendedTypeDescriptor(instance);
            }
        }

        class IncludeWorkflowDescriptionAttribute : DescriptionAttribute
        {
            IncludeWorkflowBuilder includeBuilder;

            public IncludeWorkflowDescriptionAttribute(IncludeWorkflowBuilder builder, string description)
                : base(description)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException("builder");
                }

                includeBuilder = builder;
            }

            public override string Description
            {
                get
                {
                    var description = includeBuilder.Description;
                    if (!string.IsNullOrEmpty(description))
                    {
                        return description;
                    }
                    else return DescriptionValue;
                }
            }
        }
    }
}
