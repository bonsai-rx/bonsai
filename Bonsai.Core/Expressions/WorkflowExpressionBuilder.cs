using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Dag;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that generate their output by means
    /// of an encapsulated workflow.
    /// </summary>
    [DefaultProperty("Name")]
    [WorkflowElementCategory(ElementCategory.Combinator)]
    [XmlType("Workflow", Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(WorkflowTypeDescriptionProvider))]
    public abstract class WorkflowExpressionBuilder : ExpressionBuilder, IWorkflowExpressionBuilder, INamedElement, IPropertyMappingBuilder, IRequireBuildContext
    {
        IBuildContext buildContext;
        readonly ExpressionBuilderGraph workflow;
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowExpressionBuilder"/> class.
        /// </summary>
        protected WorkflowExpressionBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowExpressionBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        protected WorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        /// <summary>
        /// Gets or sets the name of the encapsulated workflow.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the encapsulated workflow.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the encapsulated workflow.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the encapsulated workflow.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description { get; set; }

        string INamedElement.Name
        {
            get { return Name; }
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
        /// Gets the XML serializable representation of the encapsulated workflow.
        /// </summary>
        [Browsable(false)]
        [XmlElement("Workflow")]
        public ExpressionBuilderGraphDescriptor WorkflowDescriptor
        {
            get { return workflow.ToDescriptor(); }
            set
            {
                workflow.Clear();
                workflow.AddDescriptor(value);
            }
        }

        /// <summary>
        /// Gets the collection of property mappings assigned to this expression builder.
        /// Property mapping subscriptions are processed before evaluating other output generation
        /// expressions. In the case of an encapsulated workflow, mappings to nested workflow
        /// properties are also allowed.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        [XmlArrayItem("PropertyMapping")]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get
            {
                var parameterCount = workflow.GetNestedParameters().Count();
                return Range.Create(0, parameterCount);
            }
        }

        IBuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set { buildContext = value; }
        }

        internal IBuildContext BuildContext
        {
            get { return buildContext; }
        }

        /// <summary>
        /// Builds the output of the encapsulated workflow for the specified source and applies
        /// a selector taking into account any available workflow mappings.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <param name="source">
        /// The expression tree that will be used as input to the encapsulated workflow.
        /// </param>
        /// <param name="selector">
        /// A selector that will be applied to the output of the encapsulated workflow to determine
        /// the final output of the expression builder.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree that is the result of applying the encapsulated
        /// workflow to the specified input <paramref name="source"/>. Property mappings are also
        /// resolved in the correct sequence.
        /// </returns>
        protected Expression BuildWorkflow(IEnumerable<Expression> arguments, Expression source, Func<Expression, Expression> selector)
        {
            // Assign sources if available
            var nestedContext = new BuildContext(buildContext);
            var inputArguments = source != null ? Enumerable.Repeat(source, 1).Concat(arguments.Skip(1)) : arguments;
            var expression = workflow.BuildNested(inputArguments, nestedContext);
            if (nestedContext.BuildResult != null) return nestedContext.BuildResult;
            return selector(expression);
        }
    }
}
