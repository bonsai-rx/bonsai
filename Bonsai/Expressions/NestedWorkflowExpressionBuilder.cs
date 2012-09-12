using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Linq.Expressions;
using Bonsai.Dag;

namespace Bonsai.Expressions
{
    [DisplayName("NestedWorkflow")]
    [XmlType("NestedWorkflow", Namespace = Constants.XmlNamespace)]
    public class NestedWorkflowExpressionBuilder : WorkflowExpressionBuilder
    {
        public NestedWorkflowExpressionBuilder()
        {
        }

        public NestedWorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        [XmlIgnore]
        [Browsable(false)]
        public ReactiveWorkflow RuntimeWorkflow { get; private set; }

        public override Expression Build()
        {
            // Assign source if available
            var workflowInput = Workflow.Select(node => node.Value as WorkflowInputBuilder)
                                        .SingleOrDefault(builder => builder != null);
            if (workflowInput != null)
            {
                workflowInput.Source = Source;
            }

            RuntimeWorkflow = Workflow.Build();
            return RuntimeWorkflow.Connections.Single();
        }
    }
}
