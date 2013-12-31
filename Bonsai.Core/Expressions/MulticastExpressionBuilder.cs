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
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            Source = arguments.Single();
            return MulticastParameter = Expression.Parameter(Source.Type);
        }

        internal abstract Expression BuildMulticast(Expression source, LambdaExpression selector);
    }
}
