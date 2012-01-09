using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai
{
    [XmlType("SourceBase")]
    public abstract class Source : LoadableElement
    {
        protected abstract void Start();

        protected abstract void Stop();

        public IDisposable Connect()
        {
            Start();
            return Disposable.Create(Stop);
        }
    }

    public abstract class Source<T> : Source, IDisposable
    {
        bool disposed;
        readonly Subject<T> subject = new Subject<T>();

        protected Subject<T> Subject
        {
            get { return subject; }
        }

        [Browsable(false)]
        public IObservable<T> Output
        {
            get { return subject; }
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
    }
}
