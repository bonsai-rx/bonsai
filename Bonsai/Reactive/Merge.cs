using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Merges two sequences or a sequence of windows into a single sequence of elements.")]
    public class Merge
    {
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source, IObservable<TSource> other)
        {
            return source.Merge(other);
        }

        public IObservable<TSource> Process<TSource>(IObservable<IObservable<TSource>> source)
        {
            return source.Merge();
        }

        public IObservable<TSource> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.Merge(sources);
        }
    }
}
