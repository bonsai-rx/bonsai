using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("WorkflowOutput", Namespace = Constants.XmlNamespace)]
    [Description("Represents an output sequence inside a nested workflow.")]
    public class WorkflowOutputBuilder : ExpressionBuilder
    {
        [XmlIgnore]
        [Browsable(false)]
        public Expression Source { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public Expression Output { get; set; }

        public override Expression Build()
        {
            return Output = Source;
        }
    }
}
