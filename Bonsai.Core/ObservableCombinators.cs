using System;
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
    /// <summary>
    /// Provides a set of static methods to aid in writing queries over observable sequences.
    /// </summary>
    public static class ObservableCombinators
    {
        /// <summary>
        /// Generates a new observable sequence by starting a new <see cref="Thread"/> that
        /// applies the specified action on subscribed observers in a loop which recurs as
        /// fast as possible.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <param name="generator">
        /// The action that will be applied to an observer every time the generator
        /// completes a loop.
        /// </param>
        /// <returns>A new observable sequence which notifies observers in a loop.</returns>
        [Obsolete]
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

        /// <summary>
        /// Takes the single next element from the sequence every time the specified
        /// interval elapses.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="interval">
        /// The time interval after which a new element of the sequence is allowed to propagate.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public static IObservable<TSource> Gate<TSource>(this IObservable<TSource> source, TimeSpan interval)
        {
            return Gate(source, interval, Scheduler.Default);
        }

        /// <summary>
        /// Takes the single next element from the sequence every time the specified
        /// interval elapses, using the specified scheduler to run gating timers.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="interval">
        /// The time interval after which a new element of the sequence is allowed to propagate.
        /// </param>
        /// <param name="scheduler">The scheduler to run the gating timer on.</param>
        /// <returns>The gated observable sequence.</returns>
        public static IObservable<TSource> Gate<TSource>(this IObservable<TSource> source, TimeSpan interval, IScheduler scheduler)
        {
            return Gate(source, Observable.Timer(interval, scheduler));
        }

        /// <summary>
        /// Takes the single next element from the sequence every time the gate
        /// produces an element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TGate">The type of the elements in the sequence of gate events.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="gate">
        /// The sequence of gate events. Every time a new gate event is received, the single
        /// next element from <paramref name="source"/> is allowed to propagate.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public static IObservable<TSource> Gate<TSource, TGate>(this IObservable<TSource> source, IObservable<TGate> gate)
        {
            return source.Window(() => gate)
                         .SelectMany(window => window.Take(1));
        }

        /// <summary>
        /// Takes the single next element from the sequence if this element is produced
        /// within a specified time interval after the gate produces an element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TGate">The type of the elements in the sequence of gate events.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="gate">
        /// The sequence of gate events. Every time a new gate event is received, the single
        /// next element from <paramref name="source"/> is allowed to propagate if it is
        /// produced before the maximum <paramref name="timeSpan"/> elapses.
        /// </param>
        /// <param name="timeSpan">
        /// After receiving a gate event, the maximum interval that can elapse before an
        /// element from the source sequence is produced. If the element arrives after the
        /// interval elapsed, it is dropped.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public static IObservable<TSource> Gate<TSource, TGate>(this IObservable<TSource> source, IObservable<TGate> gate, TimeSpan timeSpan)
        {
            return Gate(source, gate, timeSpan, Scheduler.Default);
        }

        /// <summary>
        /// Takes the single next element from the sequence if this element is produced
        /// within a specified time interval after the gate produces an element, using
        /// the specified scheduler to run gate closing timers.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TGate">The type of the elements in the sequence of gate events.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="gate">
        /// The sequence of gate events. Every time a new gate event is received, the single
        /// next element from <paramref name="source"/> is allowed to propagate if it is
        /// produced before the maximum <paramref name="timeSpan"/> elapses.
        /// </param>
        /// <param name="timeSpan">
        /// After receiving a gate event, the maximum interval that can elapse before an
        /// element from the source sequence is produced. If the element arrives after the
        /// interval elapsed, it is dropped.
        /// </param>
        /// <param name="scheduler">The scheduler to run the gate closing timer on.</param>
        /// <returns>The gated observable sequence.</returns>
        public static IObservable<TSource> Gate<TSource, TGate>(this IObservable<TSource> source, IObservable<TGate> gate, TimeSpan timeSpan, IScheduler scheduler)
        {
            return Gate(source, gate, Observable.Timer(timeSpan, scheduler));
        }

        /// <summary>
        /// Takes the single next element from the sequence if this element is produced
        /// between a gate opening and gate closing event.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TGateOpening">The type of the elements in the sequence of gate opening events.</typeparam>
        /// <typeparam name="TGateClosing">The type of the elements in the sequence of gate closing events.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="openGate">
        /// The sequence of gate opening events. Every time a new gate event is received,
        /// the single next element from <paramref name="source"/> is allowed to propagate
        /// if it is produced before the next gate closing event.
        /// </param>
        /// <param name="closeGate">
        /// The sequence of gate closing events. Every time a new gate event is received,
        /// the single next element from <paramref name="source"/> is allowed to propagate
        /// if it is produced before the next gate closing event.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public static IObservable<TSource> Gate<TSource, TGateOpening, TGateClosing>(this IObservable<TSource> source, IObservable<TGateOpening> openGate, IObservable<TGateClosing> closeGate)
        {
            return source.Window(openGate, window => closeGate)
                         .SelectMany(window => window.Take(1));
        }

        /// <summary>
        /// Returns a connectable observable sequence that upon connection causes the <paramref name="source"/>
        /// to push results into a new fresh subject, which is created by invoking the specified
        /// <paramref name="subjectFactory"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="source">The source sequence whose elements will be pushed into the specified subject.</param>
        /// <param name="subjectFactory">
        /// The factory function used to create the subject that notifications will be pushed into.
        /// </param>
        /// <returns>The reconnectable sequence.</returns>
        [Obsolete]
        public static IConnectableObservable<TResult> Multicast<TSource, TResult>(this IObservable<TSource> source, Func<ISubject<TSource, TResult>> subjectFactory)
        {
            return MulticastReconnectable(source, subjectFactory);
        }

        /// <summary>
        /// Returns a connectable observable sequence that upon connection causes the <paramref name="source"/>
        /// to push results into a new fresh subject, which is created by invoking the specified
        /// <paramref name="subjectFactory"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="source">The source sequence whose elements will be pushed into the specified subject.</param>
        /// <param name="subjectFactory">
        /// The factory function used to create the subject that notifications will be pushed into.
        /// </param>
        /// <returns>The reconnectable sequence.</returns>
        public static IConnectableObservable<TResult> MulticastReconnectable<TSource, TResult>(this IObservable<TSource> source, Func<ISubject<TSource, TResult>> subjectFactory)
        {
            return new ReconnectableObservable<TSource, TResult>(source, subjectFactory);
        }

        /// <summary>
        /// Returns a connectable observable sequence that upon connection causes the <paramref name="source"/>
        /// to push results into a new fresh <see cref="Subject{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence whose elements will be pushed into the specified subject.</param>
        /// <returns>The reconnectable sequence.</returns>
        public static IConnectableObservable<TSource> PublishReconnectable<TSource>(this IObservable<TSource> source)
        {
            return MulticastReconnectable(source, () => new Subject<TSource>());
        }

        /// <summary>
        /// Merges an observable sequence and an enumerable sequence into one observable sequence
        /// by using the selector function.
        /// </summary>
        /// <typeparam name="TSource1">The type of the elements in the first observable source sequence.</typeparam>
        /// <typeparam name="TSource2">The type of the elements in the second observable source sequence.</typeparam>
        /// <typeparam name="TResult">
        /// The type of the elements in the result sequence, returned by the selector function.
        /// </typeparam>
        /// <param name="first">The first observable source.</param>
        /// <param name="second">The second enumerable source.</param>
        /// <param name="resultSelector">
        /// The function to invoke for each consecutive pair of elements from the first and second source.
        /// </param>
        /// <returns>
        /// An observable sequence containing the result of pairwise combining the elements
        /// of the first and second source using the specified result selector function.
        /// </returns>
        public static IObservable<TResult> Zip<TSource1, TSource2, TResult>(this IObservable<TSource1> first, IEnumerable<TSource2> second, Func<TSource1, TSource2, TResult> resultSelector)
        {
            return Observable.Create<TResult>(observer =>
            {
                var gate = new object();
                var enumerator = second.GetEnumerator();
                var subscription = first.Subscribe(x =>
                {
                    bool hasNext;
                    try
                    {
                        lock (gate) { hasNext = enumerator.MoveNext(); }
                        if (hasNext)
                        {
                            var result = resultSelector(x, enumerator.Current);
                            observer.OnNext(result);
                        }
                        else observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                observer.OnError,
                observer.OnCompleted);
                return () =>
                {
                    subscription.Dispose();
                    lock (gate) { enumerator.Dispose(); }
                };
            });
        }
    }
}
