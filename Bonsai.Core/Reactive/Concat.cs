using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Ensures that values of the second sequence are propagated only after the first sequence terminates.")]
    public class Concat
    {
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source, IObservable<TSource> other)
        {
            return source.Concat(other);
        }

        public IObservable<TSource> Process<TSource>(IObservable<IObservable<TSource>> sources)
        {
            return sources.Concat();
        }

        public IObservable<TSource> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.Concat(sources);
        }
    }
}
