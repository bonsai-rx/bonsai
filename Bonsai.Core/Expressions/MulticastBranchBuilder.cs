using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    abstract class MulticastBranchBuilder : SingleArgumentExpressionBuilder
    {
        internal MulticastBranchBuilder()
        {
        }

        internal Expression Source { get; set; }

        internal MulticastBranchExpression BranchExpression { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            Source = arguments.Single();
            return BranchExpression = new MulticastBranchExpression(Expression.Parameter(Source.Type), Source);
        }

        internal Expression BuildMulticast(Expression source, LambdaExpression selector)
        {
            var sourceType = source.Type.GetGenericArguments()[0];
            var resultType = selector.ReturnType.GetGenericArguments()[0];
            var builder = Expression.Constant(this);
            return Expression.Call(builder, "Multicast", new[] { sourceType, resultType }, source, selector);
        }

        internal abstract IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector);
    }
}
