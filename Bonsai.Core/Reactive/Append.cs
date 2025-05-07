using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that appends a value to an observable sequence.
    /// </summary>
    [Description("Appends a value to an observable sequence.")]
    public sealed class Append : OperandCombinatorExpressionBuilder
    {
        /// <inheritdoc/>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            return Expression.Call(typeof(Append), nameof(Process), new[] { right.Type }, left, right);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, TSource value)
        {
            return Observable.Create<TSource>(observer =>
            {
                var appendObserver = Observer.Create<TSource>(
                    observer.OnNext,
                    observer.OnError,
                    () =>
                    {
                        observer.OnNext(value);
                        observer.OnCompleted();
                    });
                return source.SubscribeSafe(appendObserver);
            });
        }
    }
}
