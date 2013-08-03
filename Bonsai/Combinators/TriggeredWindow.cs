using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Combinators
{
    [BinaryCombinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into non-overlapping windows. A window is closed when the second sequence produces an element.")]
    public class TriggeredWindow : LoadableElement
    {
        public IObservable<IObservable<TSource>> Process<TSource, TTrigger>(IObservable<TSource> source, IObservable<TTrigger> trigger)
        {
            return source.Window(() => trigger);
        }
    }
}
