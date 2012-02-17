using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Query", Namespace = Constants.XmlNamespace)]
    public class QueryBuilder : CombinatorBuilder
    {
        [Browsable(false)]
        public LoadableElement Combinator { get; set; }

        public override Expression Build()
        {
            var combinatorExpression = Expression.Constant(Combinator);
            return Expression.Call(combinatorExpression, "Process", null, Source);
        }
    }
}
