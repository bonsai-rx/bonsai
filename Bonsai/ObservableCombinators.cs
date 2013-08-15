﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Bonsai
{
    public static class ObservableCombinators
    {
        public static IObservable<TSource> GenerateWithThread<TSource>(Action<IObserver<TSource>> generator)
        {
            return Observable.Create<TSource>(observer =>
            {
                var running = true;
                var thread = new Thread(() =>
                {
                    while (running)
                    {
                        generator(observer);
                    }
                });

                thread.Start();
                return () =>
                {
                    running = false;
                    if (thread != Thread.CurrentThread) thread.Join();
                };
            });
        }

        public static IObservable<TSource> Gate<TSource>(this IObservable<TSource> source, TimeSpan interval)
        {
            return Gate(source, interval, Scheduler.Default);
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
            return Gate(source, sampler, timeSpan, Scheduler.Default);
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

        public static IConnectableObservable<TResult> Multicast<TSource, TResult>(this IObservable<TSource> source, Func<ISubject<TSource, TResult>> subjectSelector)
        {
            return new ReconnectableObservable<TSource, TResult>(source, subjectSelector);
        }

        public static IConnectableObservable<TSource> PublishReconnectable<TSource>(this IObservable<TSource> source)
        {
            return Multicast(source, () => new Subject<TSource>());
        }
    }
}
