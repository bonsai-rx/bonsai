using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("WorkflowInput", Namespace = Constants.XmlNamespace)]
    [Description("Represents an input sequence inside a nested workflow.")]
    public class WorkflowInputBuilder : CombinatorExpressionBuilder
    {
        public WorkflowInputBuilder()
            : base(0, 0)
        {
        }

        [XmlIgnore]
        [Browsable(false)]
        public Expression Source { get; set; }

        public override Expression Build()
        {
            return Source;
        }
    }
}
