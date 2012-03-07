using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Dag;

namespace Bonsai.Expressions
{
    [DisplayName("Workflow")]
    [XmlType("Workflow", Namespace = Constants.XmlNamespace)]
    public class WorkflowExpressionBuilder : CombinatorExpressionBuilder
    {
        readonly ExpressionBuilderGraph workflow;

        public WorkflowExpressionBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        public WorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        public string Name { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public ReactiveWorkflow RunningWorkflow { get; private set; }

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

        public override Expression Build()
        {
            // Assign source if available
            var workflowInput = workflow.Select(node => node.Value as WorkflowInputBuilder)
                                        .SingleOrDefault(builder => builder != null);
            if (workflowInput != null)
            {
                workflowInput.Source = Source;
            }

            RunningWorkflow = workflow.Build();
            return RunningWorkflow.Connections.Single();
        }
    }
}
