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
    [Description("Encapsulates complex workflow logic into a single workflow element.")]
    public class NestedWorkflowExpressionBuilder : WorkflowExpressionBuilder
    {
        public NestedWorkflowExpressionBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        public NestedWorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
            : base(workflow, minArguments: 0, maxArguments: 1)
        {
        }

        public override Expression Build()
        {
            var source = Arguments.SingleOrDefault();
            return BuildWorflow(source, expression => expression);
        }
    }
}
