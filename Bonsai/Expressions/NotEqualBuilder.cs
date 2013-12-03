using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("NotEqual", Namespace = Constants.XmlNamespace)]
    public class NotEqualBuilder : BinaryOperatorBuilder
    {
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }
    }
}
