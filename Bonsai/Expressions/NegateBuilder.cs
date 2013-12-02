using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Negate", Namespace = Constants.XmlNamespace)]
    public class NegateBuilder : SelectBuilder
    {
        protected override Expression BuildSelector(Expression expression)
        {
            return Expression.Negate(expression);
        }
    }
}
