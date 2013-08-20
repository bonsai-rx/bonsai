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
    [Description("Converts a tuple of element and timestamp into a proper timestamped type.")]
    public class CombineTimestamp
    {
        public IObservable<Timestamped<TSource>> Process<TSource>(IObservable<Tuple<TSource, DateTimeOffset>> source)
        {
            return source.Select(xs => new Timestamped<TSource>(xs.Item1, xs.Item2));
        }
    }
}
