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
    [PropertyMapping]
    [WorkflowElementCategory(ElementCategory.Nested)]
    [XmlType("Workflow", Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(WorkflowTypeDescriptionProvider))]
    public abstract class WorkflowExpressionBuilder : VariableArgumentExpressionBuilder, INamedElement
    {
        readonly ExpressionBuilderGraph workflow;
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowExpressionBuilder"/> class
        /// with the specified argument range.
        /// </summary>
        /// <param name="minArguments">The inclusive lower bound of the argument range.</param>
        /// <param name="maxArguments">The inclusive upper bound of the argument range.</param>
        protected WorkflowExpressionBuilder(int minArguments, int maxArguments)
            : this(new ExpressionBuilderGraph(), minArguments, maxArguments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowExpressionBuilder"/> class
        /// with the specified expression builder workflow and argument range.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        /// <param name="minArguments">The inclusive lower bound of the argument range.</param>
        /// <param name="maxArguments">The inclusive upper bound of the argument range.</param>
        protected WorkflowExpressionBuilder(ExpressionBuilderGraph workflow, int minArguments, int maxArguments)
            : base(minArguments, maxArguments)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        /// <summary>
        /// Gets the name of the encapsulated workflow.
        /// </summary>
        [Description("The name of the encapsulated workflow.")]
        public string Name { get; set; }

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
        [Browsable(false)]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        internal BuildContext BuildContext { get; set; }

        /// <summary>
        /// Builds the ouptut of the encapsulated workflow for the specified source and applies
        /// a selector taking into account any available workflow mappings.
        /// </summary>
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
        protected Expression BuildWorflow(Expression source, Func<Expression, Expression> selector)
        {
            // Assign source if available
            var workflowInput = Workflow.Select(node => GetWorkflowElement(node.Value) as WorkflowInputBuilder)
                                        .SingleOrDefault(builder => builder != null);
            if (workflowInput != null)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }

                workflowInput.Source = source;
            }

            var buildContext = BuildContext;
            var expression = Workflow.Build(buildContext);
            if (buildContext != null && buildContext.BuildResult != null) return buildContext.BuildResult;
            var output = selector(expression);

            var subscriptions = propertyMappings.Select(mapping =>
            {
                var inputBuilder = (from node in Workflow
                                    let property = GetWorkflowElement(node.Value) as WorkflowProperty
                                    where property != null && property.Name == mapping.Name
                                    select property).First();
                var inputExpression = Expression.Constant(inputBuilder);
                var inputMapping = new PropertyMapping("Value", mapping.Selector);
                return BuildPropertyMapping(inputExpression, inputMapping);
            });
            return BuildMappingOutput(output, subscriptions.ToArray());
        }
    }
}
