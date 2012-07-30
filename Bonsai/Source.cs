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
    public abstract class Source<TSource> : LoadableElement
    {
        [XmlIgnore]
        [Browsable(false)]
        public IObservable<TSource> Output { get; private set; }

        protected abstract IObservable<TSource> Generate();

        public override IDisposable Load()
        {
            Output = Generate();
            return base.Load();
        }

        protected override void Unload()
        {
            Output = null;
            base.Unload();
        }
    }
}
