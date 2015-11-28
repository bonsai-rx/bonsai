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
    /// between multiple branches of an expression builder workflow by means of an observable
    /// query. This is an abstract class.
    /// </summary>
    public abstract class MulticastBuilder : SingleArgumentWorkflowExpressionBuilder
    {
        internal MulticastBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        internal MulticastBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

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
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException("There must be at least one workflow input to multicast.");
            }

            // Assign input
            var selectorParameter = Expression.Parameter(source.Type);
            return BuildWorkflow(arguments, selectorParameter, selectorBody =>
            {
                var builder = Expression.Constant(this);
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                var sourceType = source.Type.GetGenericArguments()[0];
                var resultType = selector.ReturnType.GetGenericArguments()[0];
                return Expression.Call(builder, "Multicast", new[] { sourceType, resultType }, source, selector);
            });
        }

        internal abstract IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector);
    }
}
