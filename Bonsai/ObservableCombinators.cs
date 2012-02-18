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
        public static IObservable<TSource> Gate<TSource>(this IObservable<TSource> source, TimeSpan interval)
        {
            return Gate(source, interval, Scheduler.ThreadPool);
        }

        public static IObservable<TSource> Gate<TSource>(this IObservable<TSource> source, TimeSpan interval, IScheduler scheduler)
        {
            return Gate(source, Observable.Timer(interval, scheduler));
        }

        public static IObservable<TSource> Gate<TSource, TGate>(this IObservable<TSource> source, IObservable<TGate> sampler)
        {
            return source.Window(() => sampler)
                         .SelectMany(window => window.Take(1));
        }

        public static IObservable<TSource> Gate<TSource, TGate>(this IObservable<TSource> source, IObservable<TGate> sampler, TimeSpan timeSpan)
        {
            return Gate(source, sampler, timeSpan, Scheduler.ThreadPool);
        }

        public static IObservable<TSource> Gate<TSource, TGate>(this IObservable<TSource> source, IObservable<TGate> sampler, TimeSpan timeSpan, IScheduler scheduler)
        {
            return Gate(source, sampler, Observable.Timer(timeSpan, scheduler));
        }

        public static IObservable<TSource> Gate<TSource, TGateOpening, TGateClosing>(this IObservable<TSource> source, IObservable<TGateOpening> openSampler, IObservable<TGateClosing> closeSampler)
        {
            return source.Window(openSampler, window => closeSampler)
                         .SelectMany(window => window.Take(1));
        }
    }
}
