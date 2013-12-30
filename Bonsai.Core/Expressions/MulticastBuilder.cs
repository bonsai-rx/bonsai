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
    public abstract class MulticastBuilder : MulticastExpressionBuilder
    {
        internal MulticastBuilder()
        {
        }

        internal override Expression BuildMulticast(Expression source, LambdaExpression selector)
        {
            var sourceType = source.Type.GetGenericArguments()[0];
            var resultType = selector.ReturnType.GetGenericArguments()[0];
            var builder = Expression.Constant(this);
            return Expression.Call(builder, "Multicast", new[] { sourceType, resultType }, source, selector);
        }

        internal abstract IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector);
    }
}
