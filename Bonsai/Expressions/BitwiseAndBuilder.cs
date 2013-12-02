using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("BitwiseAnd", Namespace = Constants.XmlNamespace)]
    public class BitwiseAndBuilder : BinaryOperatorBuilder
    {
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            return Expression.And(left, right);
        }
    }
}
