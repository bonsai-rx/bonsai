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
    [Description("Propagates the sequence that responds first and ignores the other.")]
    public class Amb : LoadableElement
    {
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source, IObservable<TSource> other)
        {
            return source.Amb(other);
        }
    }
}
