using System;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    class PublishBranchBuilder : MulticastBranchBuilder
    {
        internal override IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            return source.Publish(selector);
        }
    }
}
