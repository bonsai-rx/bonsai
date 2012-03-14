using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai
{
    [XmlType("SourceBase", Namespace = Constants.XmlNamespace)]
    public abstract class Source : LoadableElement
    {
        protected abstract void Start();

        protected abstract void Stop();
    }

    public abstract class Source<T> : Source, IDisposable
    {
        bool disposed;
        readonly Subject<T> subject;
        readonly IObservable<T> output;

        public Source()
        {
            subject = new Subject<T>();
            output = new ConnectableSource(this).RefCount();
        }

        protected Subject<T> Subject
        {
            get { return subject; }
        }

        [Browsable(false)]
        public IObservable<T> Output
        {
            get { return output; }
        }

        ~Source()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    subject.Dispose();
                    disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        class ConnectableSource : IConnectableObservable<T>
        {
            readonly Source<T> source;

            public ConnectableSource(Source<T> source)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }

                this.source = source;
            }

            public IDisposable Connect()
            {
                source.Start();
                return Disposable.Create(source.Stop);
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return source.Subject.Subscribe(observer);
            }
        }
    }
}
