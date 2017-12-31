﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SystemPath = System.IO.Path;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that uses an encapsulated workflow stored
    /// externally to generate its output.
    /// </summary>
    [DefaultProperty("Path")]
    [WorkflowElementCategory(ElementCategory.Nested)]
    [XmlType("IncludeWorkflow", Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(IncludeWorkflowTypeDescriptionProvider))]
    public sealed class IncludeWorkflowBuilder : VariableArgumentExpressionBuilder, IGroupWorkflowBuilder, INamedElement, IRequireBuildContext
    {
        static readonly XmlElement[] EmptyProperties = new XmlElement[0];
        XmlElement[] xmlProperties;

        IBuildContext buildContext;
        ExpressionBuilderGraph workflow;
        readonly bool inspectWorkflow;
        DateTime writeTime;
        string workflowPath;
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
            if (workflow != null)
            {
                workflow = inspect ? workflow.ToInspectableGraph() : workflow.FromInspectableGraph();
            }

            writeTime = builder.writeTime;
            name = builder.name;
            path = builder.path;
            description = builder.description;
            workflowPath = builder.workflowPath;
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
                EnsureWorkflow();
                xmlProperties = null;
            }
        }

        /// <summary>
        /// Gets or sets the path of the workflow to include.
        /// </summary>
        [XmlAttribute]
        [FileNameFilter("Bonsai Files (*.bonsai)|*.bonsai")]
        [Description("The path of the workflow to include.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                writeTime = DateTime.MinValue;
                workflowPath = ResolvePath(path);
                name = SystemPath.GetFileNameWithoutExtension(workflowPath);
            }
        }

        /// <summary>
        /// Gets or sets the XML representation of externalized property values from the included workflow.
        /// </summary>
        [XmlAnyElement]
        [Browsable(false)]
        public XmlElement[] PropertiesXml
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

        XmlElement[] GetXmlProperties()
        {
            return (from property in TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>()
                    let externalizedProperty = property as ExternalizedPropertyDescriptor
                    where externalizedProperty != null && !externalizedProperty.IsReadOnly
                    select SerializeProperty(externalizedProperty))
                    .ToArray();
        }

        void SetXmlProperties(XmlElement[] xmlProperties)
        {
            var properties = (from property in TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>()
                              let externalizedProperty = property as ExternalizedPropertyDescriptor
                              where externalizedProperty != null && !externalizedProperty.IsReadOnly
                              select externalizedProperty)
                              .ToDictionary(property => property.Name);
            for (int i = 0; i < xmlProperties.Length; i++)
            {
                ExternalizedPropertyDescriptor property;
                if (properties.TryGetValue(xmlProperties[i].Name, out property))
                {
                    DeserializeProperty(xmlProperties[i], property);
                }
            }
        }

        void DeserializeProperty(XmlElement element, PropertyDescriptor property)
        {
            var xmlRoot = new XmlRootAttribute(property.Name) { Namespace = Constants.XmlNamespace };
            var serializer = new XmlSerializer(property.PropertyType, xmlRoot);
            using (var reader = new StringReader(element.OuterXml))
            {
                var value = serializer.Deserialize(reader);
                property.SetValue(this, value);
            }
        }

        XmlElement SerializeProperty(PropertyDescriptor property)
        {
            var document = new XmlDocument();
            var xmlRoot = new XmlRootAttribute(property.Name) { Namespace = Constants.XmlNamespace };
            var serializer = new XmlSerializer(property.PropertyType, xmlRoot);
            using (var writer = document.CreateNavigator().AppendChild())
            {
                serializer.Serialize(writer, property.GetValue(this));
            }
            return document.DocumentElement;
        }

        static string GetWorkflowPath(string pathLocation, string path)
        {
            return SystemPath.Combine(pathLocation, path) + Constants.BonsaiExtension;
        }

        static string ResolvePath(string path)
        {
            const string PathEnvironmentVariable = "PATH";
            if (string.IsNullOrEmpty(path) || SystemPath.HasExtension(path)) return path;

            var workflowPath = GetWorkflowPath(Environment.CurrentDirectory, path);
            if (File.Exists(workflowPath)) return workflowPath;

            var pathLocations = Environment.GetEnvironmentVariable(PathEnvironmentVariable).Split(SystemPath.PathSeparator);
            for (int i = 0; i < pathLocations.Length; i++)
            {
                workflowPath = GetWorkflowPath(pathLocations[i], path);
                if (File.Exists(workflowPath)) return workflowPath;
            }

            return string.Empty;
        }

        void EnsureWorkflow()
        {
            var path = workflowPath;
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

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                workflow = null;
                description = null;
                SetArgumentRange(0, 1);
            }
            else
            {
                var lastWriteTime = File.GetLastWriteTime(path);
                if (workflow == null || lastWriteTime > writeTime)
                {
                    var properties = workflow != null ? GetXmlProperties() : xmlProperties;
                    using (var reader = XmlReader.Create(path))
                    {
                        var builder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
                        description = builder.Description;
                        workflow = builder.Workflow;
                        writeTime = lastWriteTime;

                        var parameterCount = workflow.GetNestedParameters().Count();
                        SetArgumentRange(parameterCount, parameterCount);
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
                    return arguments.FirstOrDefault() ?? EmptyExpression;
                }
                else throw new InvalidOperationException("The specified workflow could not be found.");
            }

            var includeContext = new IncludeContext(buildContext, workflowPath);
            return workflow.BuildNested(arguments, includeContext);
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
                    return new WorkflowTypeDescriptor(instance, description);
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
