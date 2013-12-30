using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that handle sharing of sequences
    /// between multiple branches of an expression builder workflow. This is an abstract class.
    /// </summary>
    public abstract class MulticastExpressionBuilder : SingleArgumentExpressionBuilder
    {
        internal MulticastExpressionBuilder()
        {
        }

        internal Expression Source { get; set; }

        internal ParameterExpression MulticastParameter { get; set; }

        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be passed on
        /// to other builders in the workflow.
        /// </summary>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build()
        {
            Source = Arguments.Single();
            return MulticastParameter = Expression.Parameter(Source.Type);
        }

        internal abstract Expression BuildMulticast(Expression source, LambdaExpression selector);
    }
}
