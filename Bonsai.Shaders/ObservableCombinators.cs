using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class ObservableCombinators
    {
        public static IConnectableObservable<TSource> ReplayReconnectable<TSource>(this IObservable<TSource> source)
        {
            return Bonsai.ObservableCombinators.Multicast(source, () => new ReplaySubject<TSource>());
        }

        public static IObservable<TResult> CombineEither<TSource1, TSource2, TResult>(
            this IObservable<TSource1> first,
            IObservable<TSource2> second,
            Func<TSource1, TSource2, TResult> resultSelector)
        {
            return first.Publish(ps1 => second.Publish(ps2 =>
                ps1.CombineLatest(ps2, resultSelector)
                   .TakeUntil(ps1.LastOrDefaultAsync())
                   .TakeUntil(ps2.LastOrDefaultAsync())));
        }
    }
}
