using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Combinators
{
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Removes timestamp information from the elements of the sequence.")]
    public class RemoveTimestamp
    {
        public IObservable<TSource> Process<TSource>(IObservable<Timestamped<TSource>> source)
        {
            return source.Select(xs => xs.Value);
        }
    }
}
