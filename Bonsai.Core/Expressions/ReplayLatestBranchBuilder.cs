using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    class ReplayLatestBranchBuilder : MulticastBranchBuilder
    {
        internal override IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            return selector(source.Replay(1, Scheduler.Immediate).RefCount());
        }
    }
}
