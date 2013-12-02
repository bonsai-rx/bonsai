using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    public abstract class BinaryOperatorBuilder : SelectBuilder
    {
        protected abstract Expression BuildSelector(Expression left, Expression right);

        protected override Expression BuildSelector(Expression expression)
        {
            var left = ExpressionHelper.MemberAccess(expression, "Item1");
            var right = ExpressionHelper.MemberAccess(expression, "Item2");
            if (left.Type != right.Type && left.Type.IsPrimitive && right.Type.IsPrimitive)
            {
                var comparison = CompareConversion(left.Type, right.Type, typeof(object));
                if (comparison < 0) left = Expression.Convert(left, right.Type);
                else right = Expression.Convert(right, left.Type);
            }

            return BuildSelector(left, right);
        }
    }
}
