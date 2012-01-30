using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Bonsai
{
    public static class ObservableCombinators
    {
        public static IObservable<T> RateThrottle<T>(this IObservable<T> source, TimeSpan interval)
        {
            return RateThrottle<T>(source, interval, Scheduler.ThreadPool);
        }

        public static IObservable<T> RateThrottle<T>(this IObservable<T> source, TimeSpan interval, IScheduler scheduler)
        {
            return Observable.Create<T>(o =>
            {
                DateTimeOffset last = DateTimeOffset.MinValue;
                return source.Subscribe(x =>
                {
                    DateTimeOffset now = scheduler.Now;
                    if (now - last >= interval)
                    {
                        last = now;
                        o.OnNext(x);
                    }
                }, o.OnError, o.OnCompleted);
            });
        }
    }
}
