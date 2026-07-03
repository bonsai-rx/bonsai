using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that combines the latest values from the source sequences only
    /// when the first sequence produces an element.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Combines the latest values from the source sequences only when the first sequence produces an element.")]
    public class WithLatestFrom
    {
        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a pair with
        /// the latest source elements only when the first observable sequence produces an
        /// element.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The first observable sequence.</param>
        /// <param name="other">The other observable sequence.</param>
        /// <returns>
        /// An observable sequence containing the result of combining the latest elements of the
        /// sources into pairs only when the first sequence produces an element.
        /// </returns>
        public IObservable<Tuple<TSource, TOther>> Process<TSource, TOther>(
            IObservable<TSource> source,
            IObservable<TOther> other)
        {
            return Observable.Create<Tuple<TSource, TOther>>(observer =>
            {
                var gate = new object();
                var latestGate = new object();
                var hasLatest = false;
                var latest = default(TOther);
                var otherDisposable = new SingleAssignmentDisposable();
                var otherObserver = Observer.Create<TOther>(
                    value =>
                    {
                        lock (latestGate)
                        {
                            latest = value;
                        }

                        if (!Volatile.Read(ref hasLatest))
                        {
                            Volatile.Write(ref hasLatest, true);
                        }
                    },
                    error =>
                    {
                        lock (gate)
                        {
                            observer.OnError(error);
                        }
                    },
                    otherDisposable.Dispose);
                otherDisposable.Disposable = other.SubscribeSafe(otherObserver);

                var sourceObserver = Observer.Create<TSource>(
                    value =>
                    {
                        if (Volatile.Read(ref hasLatest))
                        {
                            TOther latestValue;
                            lock (latestGate)
                            {
                                latestValue = latest;
                            }

                            lock (gate)
                            {
                                observer.OnNext(Tuple.Create(value, latestValue));
                            }
                        }
                    },
                    error =>
                    {
                        lock (gate)
                        {
                            observer.OnError(error);
                        }
                    },
                    () =>
                    {
                        lock (gate)
                        {
                            observer.OnCompleted();
                        }
                    });
                var sourceDisposable = source.SubscribeSafe(sourceObserver);
                return new CompositeDisposable(otherDisposable, sourceDisposable);
            });
        }
    }
}
