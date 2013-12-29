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
    [PropertyMapping]
    [WorkflowElementCategory(ElementCategory.Nested)]
    [XmlType("Workflow", Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(WorkflowTypeDescriptionProvider))]
    public abstract class WorkflowExpressionBuilder : VariableArgumentExpressionBuilder, INamedElement
    {
        readonly ExpressionBuilderGraph workflow;
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        protected WorkflowExpressionBuilder(int minArguments, int maxArguments)
            : this(new ExpressionBuilderGraph(), minArguments, maxArguments)
        {
        }

        protected WorkflowExpressionBuilder(ExpressionBuilderGraph workflow, int minArguments, int maxArguments)
            : base(minArguments, maxArguments)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        [Description("The name of the encapsulated workflow.")]
        public string Name { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

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

        [Browsable(false)]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        internal BuildContext BuildContext { get; set; }

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
