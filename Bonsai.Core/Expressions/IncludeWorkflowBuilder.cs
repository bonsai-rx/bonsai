using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
    [DefaultProperty(nameof(Path))]
    [WorkflowElementCategory(ElementCategory.Workflow)]
    [XmlType("IncludeWorkflow", Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(IncludeWorkflowTypeDescriptionProvider))]
    public sealed class IncludeWorkflowBuilder : VariableArgumentExpressionBuilder, IGroupWorkflowBuilder, INamedElement, IRequireBuildContext
    {
        const char AssemblySeparator = ':';
        internal const string BuildUriPrefix = "::build:";
        const string PlaceholderSerializerPropertyName = "__Property__";
        static readonly XElement[] EmptyProperties = new XElement[0];
        static readonly XmlSerializerNamespaces DefaultSerializerNamespaces = GetXmlSerializerNamespaces();

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
            : base(minArguments: 0, maxArguments: 1)
        {
            inspectWorkflow = inspect;
            workflow = builder.workflow;
            if (workflow != null)
            {
                var parameterCount = workflow.GetNestedParameters().Count();
                SetArgumentRange(0, parameterCount);
                if (inspect != builder.inspectWorkflow)
                {
                    workflow = inspect ? workflow.ToInspectableGraph() : workflow.FromInspectableGraph();
                }
            }

            writeTime = builder.writeTime;
            name = builder.name;
            path = builder.path;
            description = builder.description;
            InternalXmlProperties = builder.InternalXmlProperties;
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

        /// <summary>
        /// Gets the range of input arguments the included workflow accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get
            {
                if (workflow == null)
                {
                    try { EnsureWorkflow(null); }
                    catch (Exception) { }
                }
                return base.ArgumentRange;
            }
        }

        IBuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set
            {
                buildContext = value;
                if (buildContext != null)
                {
                    EnsureWorkflow(buildContext);
                    InternalXmlProperties = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the path of the workflow to include.
        /// </summary>
        [XmlAttribute]
        [DesignOnly(true)]
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
                if (InternalXmlProperties != null) return InternalXmlProperties;
                else if (workflow != null)
                {
                    return GetXmlProperties(workflow);
                }
                else return EmptyProperties;
            }
            set { InternalXmlProperties = value; }
        }

        internal XElement[] InternalXmlProperties { get; private set; }

        static XmlSerializerNamespaces GetXmlSerializerNamespaces()
        {
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add("xsi", XmlSchema.InstanceNamespace);
            return serializerNamespaces;
        }

        static XElement[] GetXmlProperties(ExpressionBuilderGraph workflow)
        {
            if (workflow is null)
                throw new ArgumentNullException(nameof(workflow));

            var properties = TypeDescriptor.GetProperties(workflow);
            return GetXmlSerializableProperties(properties)
                .Select(property => SerializeProperty(workflow, property))
                .Where(element => element != null)
                .ToArray();
        }

        static void SetXmlProperties(ExpressionBuilderGraph workflow, XElement[] xmlProperties)
        {
            if (workflow is null)
                throw new ArgumentNullException(nameof(workflow));

            var properties = TypeDescriptor.GetProperties(workflow);
            var serializableProperties = GetXmlSerializableProperties(properties).ToDictionary(property => property.Name);
            for (int i = 0; i < xmlProperties.Length; i++)
            {
                if (serializableProperties.TryGetValue(xmlProperties[i].Name.LocalName, out ExternalizedPropertyDescriptor property))
                {
                    if (xmlProperties[i].NodeType == XmlNodeType.Text)
                    {
                        var value = xmlProperties[i].Value;
                        property = (ExternalizedPropertyDescriptor)properties[property.Name];
                        property.SetValue(workflow, property.Converter.ConvertFromInvariantString(value));
                    }
                    else DeserializeProperty(workflow, xmlProperties[i], property);
                }
            }
        }

        static IEnumerable<ExternalizedPropertyDescriptor> GetXmlSerializableProperties(PropertyDescriptorCollection properties)
        {
            return from property in properties.Cast<PropertyDescriptor>()
                   let externalizedProperty = EnsureXmlSerializable(property as ExternalizedPropertyDescriptor)
                   where externalizedProperty != null && (!externalizedProperty.IsReadOnly || ExpressionHelper.IsCollectionType(externalizedProperty.PropertyType))
                   select externalizedProperty;
        }

        static ExternalizedPropertyDescriptor EnsureXmlSerializable(ExternalizedPropertyDescriptor descriptor)
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

        static void DeserializeProperty(ExpressionBuilderGraph workflow, XElement element, PropertyDescriptor property)
        {
            if (property.PropertyType == typeof(XElement))
            {
                property.SetValue(workflow, element);
                return;
            }

            var previousName = element.Name;
            element.Name = XName.Get(PlaceholderSerializerPropertyName, element.Name.NamespaceName);
            try
            {
                var serializer = PropertySerializer.GetXmlSerializer(property.PropertyType);
                using var reader = element.CreateReader();
                var value = serializer.Deserialize(reader);
                if (property.IsReadOnly)
                {
                    var collection = (IList)property.GetValue(workflow);
                    if (collection == null)
                    {
                        throw new InvalidOperationException("Collection reference not set to an instance of an object.");
                    }

                    collection.Clear();
                    if (value is IEnumerable collectionElements)
                    {
                        foreach (var collectionElement in collectionElements)
                        {
                            collection.Add(collectionElement);
                        }
                    }
                }
                else property.SetValue(workflow, value);
            }
            finally
            {
                element.Name = previousName;
            }
        }

        static XElement SerializeProperty(ExpressionBuilderGraph workflow, ExternalizedPropertyDescriptor property)
        {
            var value = property.GetValue(workflow, out bool allEqual);
            if (!allEqual) return null;

            var document = new XDocument();
            var serializer = PropertySerializer.GetXmlSerializer(property.PropertyType);
            using (var writer = document.CreateWriter())
            {
                serializer.Serialize(writer, value, DefaultSerializerNamespaces);
            }

            var element = document.Root;
            element.Name = XName.Get(property.Name, element.Name.NamespaceName);
            return element;
        }

        static string GetWorkflowPath(string path)
        {
            if (SystemPath.GetExtension(path) == string.Empty)
            {
                return SystemPath.ChangeExtension(path, Constants.BonsaiExtension);
            }

            return path;
        }

        internal static bool IsEmbeddedResourcePath(string path)
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

        internal static Stream GetWorkflowStream(string path, bool embeddedResource)
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
                try { return File.OpenRead(path); }
                catch (FileNotFoundException ex)
                {
                    throw new InvalidOperationException("The specified workflow could not be found.", ex);
                }
            }
        }

        void EnsureWorkflow(IBuildContext buildContext)
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
                var baseUri = buildContext != null ? $"{BuildUriPrefix}{path}" : path;
                var lastWriteTime = embeddedResource ? DateTime.MaxValue : File.GetLastWriteTime(path);
                if (workflow == null || lastWriteTime > writeTime)
                {
                    WorkflowBuilder builder;
                    var properties = workflow != null ? GetXmlProperties(workflow) : InternalXmlProperties;
                    using (var stream = GetWorkflowStream(path, embeddedResource))
                    using (var reader = XmlReader.Create(stream, null, baseUri))
                    {
                        reader.MoveToContent();
                        var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
                        builder = (WorkflowBuilder)serializer.Deserialize(reader);
                    }

                    if (properties != null)
                    {
                        SetXmlProperties(builder.Workflow, properties);
                    }

                    var parameterCount = builder.Workflow.GetNestedParameters().Count();
                    workflow = inspectWorkflow ? builder.Workflow.ToInspectableGraph() : builder.Workflow;
                    description = builder.Description;
                    SetArgumentRange(0, parameterCount);
                    writeTime = lastWriteTime;
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

        // We serialize all properties using the same placeholder root name to allow us to reuse
        // a single serializer for each different property type. This means we need to rename
        // the actual XElement name before or after serialization and deserialization.
        static class PropertySerializer
        {
            static readonly Dictionary<Type, XmlSerializer> serializerCache = new();
            static readonly object cacheLock = new();

            internal static XmlSerializer GetXmlSerializer(Type type)
            {
                XmlSerializer serializer;
                lock (cacheLock)
                {
                    if (!serializerCache.TryGetValue(type, out serializer))
                    {
                        var xmlRoot = new XmlRootAttribute(PlaceholderSerializerPropertyName) { Namespace = Constants.XmlNamespace };
                        serializer = new XmlSerializer(type, xmlRoot);
                        serializerCache.Add(type, serializer);
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
            readonly IncludeWorkflowBuilder includeBuilder;

            public IncludeWorkflowDescriptionAttribute(IncludeWorkflowBuilder builder, string description)
                : base(description)
            {
                includeBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
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

        class IncludeWorkflowDeferredPropertyDescriptor : PropertyDescriptor
        {
            readonly ExternalizedPropertyDescriptor property;

            public IncludeWorkflowDeferredPropertyDescriptor(ExternalizedPropertyDescriptor externalizedProperty)
                : base(externalizedProperty)
            {
                property = externalizedProperty;
            }

            public override Type ComponentType
            {
                get { return typeof(IncludeWorkflowBuilder); }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(XElement); }
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                var workflow = component switch
                {
                    IncludeWorkflowBuilder includeWorkflow => includeWorkflow.Workflow,
                    ExpressionBuilderGraph workflowComponent => workflowComponent,
                    _ => throw new ArgumentException("Incompatible component type in workflow property assignment.", nameof(component))
                };

                var serializableProperty = EnsureXmlSerializable(property);
                if (serializableProperty != null)
                {
                    return SerializeProperty(workflow, serializableProperty);
                }

                return null;
            }

            public override void ResetValue(object component)
            {
                throw new NotSupportedException();
            }

            public override void SetValue(object component, object value)
            {
                if (value is not XElement element)
                {
                    throw new ArgumentException("Incompatible types found in workflow property assignment.", nameof(value));
                }

                var workflow = component switch
                {
                    IncludeWorkflowBuilder includeWorkflow => includeWorkflow.Workflow,
                    ExpressionBuilderGraph workflowComponent => workflowComponent,
                    _ => throw new ArgumentException("Incompatible component type in workflow property assignment.", nameof(component))
                };

                var serializableProperty = EnsureXmlSerializable(property);
                if (serializableProperty != null)
                {
                    DeserializeProperty(workflow, element, serializableProperty);
                }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }
        }

        struct DescriptorInfo
        {
            public PropertyDescriptor Property;
            public object Component;
        }

        static void GetDescriptorProperties(List<DescriptorInfo> descriptors, out PropertyDescriptor[] properties, out object[] components)
        {
            properties = new PropertyDescriptor[descriptors.Count];
            components = new object[descriptors.Count];
            for (int i = 0; i < descriptors.Count; i++)
            {
                properties[i] = descriptors[i].Property;
                components[i] = descriptors[i].Component;
            }
        }

        internal static ExternalizedPropertyDescriptor GetDeferredProperties(ExternalizedMapping property, PropertyDescriptorCollection[] targetProperties, object[] targetComponents)
        {
            var propertyType = default(Type);
            var xmlDescriptors = new List<DescriptorInfo>();
            var instanceDescriptors = new List<DescriptorInfo>();

            for (int i = 0; i < targetProperties.Length; i++)
            {
                DescriptorInfo descriptor;
                descriptor.Property = targetProperties[i][property.Name];
                descriptor.Component = targetComponents[i];
                if (descriptor.Property is XmlPropertyDescriptor)
                {
                    xmlDescriptors.Add(descriptor);
                    continue;
                }

                if (propertyType == null)
                {
                    propertyType = descriptor.Property.PropertyType;
                }
                else if (descriptor.Property.PropertyType != propertyType)
                {
                    return null;
                }

                instanceDescriptors.Add(descriptor);
            }

            if (instanceDescriptors.Count > 0)
            {
                DescriptorInfo instanceDescriptor;
                GetDescriptorProperties(instanceDescriptors, out PropertyDescriptor[] instanceProperties, out object[] instanceComponents);
                var externalizedProperty = new ExternalizedPropertyDescriptor(property, instanceProperties, instanceComponents);
                instanceDescriptor.Property = new IncludeWorkflowDeferredPropertyDescriptor(externalizedProperty);
                instanceDescriptor.Component = null;
                xmlDescriptors.Add(instanceDescriptor);
            }

            GetDescriptorProperties(xmlDescriptors, out PropertyDescriptor[] xmlProperties, out object[] xmlComponents);
            return new ExternalizedPropertyDescriptor(property, xmlProperties, xmlComponents);
        }
    }
}
