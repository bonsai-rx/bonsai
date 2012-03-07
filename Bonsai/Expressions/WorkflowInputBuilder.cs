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
    public class WorkflowInputBuilder : ExpressionBuilder
    {
        [XmlIgnore]
        [Browsable(false)]
        public Expression Source { get; set; }

        public override Expression Build()
        {
            return Source;
        }
    }
}
