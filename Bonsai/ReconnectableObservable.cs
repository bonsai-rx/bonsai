using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Bonsai
{
    class ReconnectableObservable<TSource, TResult> : IConnectableObservable<TResult>
    {
        readonly object gate;
        readonly IObservable<TSource> observableSource;
        readonly Func<ISubject<TSource, TResult>> subjectFactory;
        IConnectableObservable<TResult> connectableSource;

        public ReconnectableObservable(IObservable<TSource> source, Func<ISubject<TSource, TResult>> subjectSelector)
        {
            subjectFactory = subjectSelector;
            observableSource = source.AsObservable();
            gate = new object();
        }

        public IDisposable Connect()
        {
            lock (gate)
            {
                EnsureConnectableSource();
                var connection = connectableSource.Connect();
                return Disposable.Create(() =>
                {
                    lock (gate)
                    {
                        using (connection)
                        {
                            connectableSource = null;
                        }
                    }
                });
            }
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            lock (gate)
            {
                EnsureConnectableSource();
                return connectableSource.Subscribe(observer);
            }
        }

        private void EnsureConnectableSource()
        {
            if (connectableSource == null)
            {
                var subject = subjectFactory();
                connectableSource = observableSource.Multicast(subject);
            }
        }
    }
}
