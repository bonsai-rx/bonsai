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
    [Source]
    public abstract class Source<TSource> : LoadableElement
    {
        public abstract IObservable<TSource> Generate();
    }
}
