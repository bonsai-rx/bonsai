using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    [XmlType("NullSink", Namespace = Constants.XmlNamespace)]
    public class NullSinkBuilder : CombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            return Source;
        }
    }
}
