using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace Bonsai.Combinators
{
    [BinaryCombinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Combines pairwise values from both input sequences only when both sequences produce a new element.")]
    public class Zip
    {
        public IObservable<Tuple<TSource, TOther>> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Zip(other, (xs, ys) => Tuple.Create(xs, ys));
        }
    }
}
