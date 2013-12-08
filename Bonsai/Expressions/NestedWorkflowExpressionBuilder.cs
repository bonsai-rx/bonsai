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
        static readonly Range<int> argumentRange = Range.Create(0, 2);

        public NestedWorkflowExpressionBuilder()
        {
        }

        public NestedWorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        public override Expression Build()
        {
            var source = Arguments.Values.SingleOrDefault();
            return BuildWorflow(source, expression => expression);
        }
    }
}
