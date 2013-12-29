using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that define a simple binary operator
    /// on paired elements of an observable sequence. This is an abstract class.
    /// </summary>
    public abstract class BinaryOperatorBuilder : SelectBuilder
    {
        /// <summary>
        /// When overridden in a derived class, returns the expression that applies a binary
        /// operator to the left and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies the binary operator to the left
        /// and right parameters.
        /// </returns>
        protected abstract Expression BuildSelector(Expression left, Expression right);

        /// <summary>
        /// Returns the expression that maps the specified input parameter to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that maps the input parameter to the
        /// selector result.
        /// </returns>
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
