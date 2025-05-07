using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that prepends a value to an observable sequence.
    /// </summary>
    [Description("Prepends a value to an observable sequence.")]
    public sealed class Prepend : OperandCombinatorExpressionBuilder
    {
        /// <inheritdoc/>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            return Expression.Call(typeof(Prepend), nameof(Process), new[] { right.Type }, left, right);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, TSource value)
        {
            return Observable.Create<TSource>(observer =>
            {
                observer.OnNext(value);
                return source.SubscribeSafe(observer);
            });
        }
    }
}
