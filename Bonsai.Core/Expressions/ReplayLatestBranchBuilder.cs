using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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
