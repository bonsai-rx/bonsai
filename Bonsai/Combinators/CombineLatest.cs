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
    [Description("Combines values from both input sequences whenever one of the sequences produces an element.")]
    public class CombineLatest : LoadableElement
    {
        public IObservable<Tuple<TSource, TOther>> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.CombineLatest(other, (xs, ys) => Tuple.Create(xs, ys));
        }
    }
}
