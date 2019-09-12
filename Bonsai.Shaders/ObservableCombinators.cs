using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
            return source.MulticastReconnectable(() => new ReplaySubject<TSource>());
        }

        public static IObservable<TResult> CombineEither<TSource1, TSource2, TResult>(
            this IObservable<TSource1> first,
            IObservable<TSource2> second,
            Func<TSource1, TSource2, TResult> resultSelector)
        {
            return Observable.Create<TResult>(observer =>
            {
                var disposable1 = new SingleAssignmentDisposable();
                var disposable2 = new SingleAssignmentDisposable();
                disposable1.Disposable = second.SubscribeSafe(Observer.Create<TSource2>(
                    x2 =>
                    {
                        disposable2.Disposable = first
                            .Select(x1 => resultSelector(x1, x2))
                            .SubscribeSafe(observer);
                    },
                    observer.OnError,
                    observer.OnCompleted));
                return new CompositeDisposable(disposable2, disposable1);
            });
        }
    }
}
