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

    public abstract class Source<T> : Source
    {
        Subject<T> subject;
        readonly IObservable<T> output;

        public Source()
        {
            output = new ConnectableSource(this).RefCount();
        }

        protected Subject<T> Subject
        {
            get { return subject; }
        }

        public override IDisposable Load()
        {
            subject = new Subject<T>();
            return base.Load();
        }

        protected override void Unload()
        {
            subject.Dispose();
            subject = null;
            base.Unload();
        }

        [Browsable(false)]
        public IObservable<T> Output
        {
            get { return output; }
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
                if (source.Subject == null)
                {
                    throw new InvalidOperationException("Cannot subscribe to unloaded data source.");
                }

                return source.Subject.Subscribe(observer);
            }
        }
    }
}
