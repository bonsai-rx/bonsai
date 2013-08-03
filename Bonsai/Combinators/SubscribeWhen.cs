using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Combinators
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Subscribes to the first sequence of values only when the second sequence produces an element.")]
    public class SubscribeWhen : BinaryCombinator
    {
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return other.Take(1).SelectMany(x => source);
        }
    }
}
