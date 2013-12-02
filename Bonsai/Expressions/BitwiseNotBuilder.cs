using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("BitwiseNot", Namespace = Constants.XmlNamespace)]
    public class BitwiseNotBuilder : SelectBuilder
    {
        protected override Expression BuildSelector(Expression expression)
        {
            return Expression.Not(expression);
        }
    }
}
