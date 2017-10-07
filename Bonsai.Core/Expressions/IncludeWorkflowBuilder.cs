using System;
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
    [WorkflowElementCategory(ElementCategory.Nested)]
    [XmlType("IncludeWorkflow", Namespace = Constants.XmlNamespace)]
    [Description("Includes an encapsulated workflow from the specified path.")]
    [TypeDescriptionProvider(typeof(IncludeWorkflowTypeDescriptionProvider))]
    public sealed class IncludeWorkflowBuilder : VariableArgumentExpressionBuilder, INamedElement, IRequireBuildContext
    {
        BuildContext buildContext;
        ExpressionBuilderGraph workflow;
        readonly bool inspectWorkflow;
        DateTime writeTime;
        string workflowPath;
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
            workflowPath = builder.workflowPath;
        }

        string INamedElement.Name
        {
            get { return workflow != null ? name : null; }
        }

        BuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set
            {
                EnsureWorkflow();
                buildContext = value;
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
                workflow = null;
                workflowPath = ResolvePath(path);
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
                EnsureWorkflow();
                return (from property in TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>()
                        let externalizedProperty = property as ExternalizedPropertyDescriptor
                        where externalizedProperty != null && !externalizedProperty.IsReadOnly
                        select SerializeProperty(externalizedProperty))
                        .ToArray();
            }
            set
            {
                EnsureWorkflow();
                var properties = (from property in TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>()
                                  let externalizedProperty = property as ExternalizedPropertyDescriptor
                                  where externalizedProperty != null && !externalizedProperty.IsReadOnly
                                  select externalizedProperty)
                                  .ToArray();
                for (int i = 0; i < properties.Length; i++)
                {
                    DeserializeProperty(value[i], properties[i]);
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
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                workflow = null;
                SetArgumentRange(0, 1);
            }
            else
            {
                var lastWriteTime = File.GetLastWriteTime(path);
                if (workflow == null || lastWriteTime > writeTime)
                {
                    using (var reader = XmlReader.Create(path))
                    {
                        var builder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
                        name = SystemPath.GetFileNameWithoutExtension(path);
                        workflow = builder.Workflow;
                        writeTime = lastWriteTime;

                        var parameterCount = workflow.GetNestedParameters().Count();
                        SetArgumentRange(parameterCount, parameterCount);
                        if (inspectWorkflow)
                        {
                            workflow = workflow.ToInspectableGraph();
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

            return workflow.BuildNested(arguments, buildContext);
        }

        class IncludeWorkflowTypeDescriptionProvider : TypeDescriptionProvider
        {
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
                    var workflow = builder.workflow;
                    if (workflow != null) return new WorkflowTypeDescriptor(workflow);
                }

                return base.GetExtendedTypeDescriptor(instance);
            }
        }
    }
}
