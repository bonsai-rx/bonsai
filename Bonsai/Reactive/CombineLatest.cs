using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace Bonsai.Reactive
{
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Combines values from the input sequences whenever any of the sequences produces an element.")]
    public class CombineLatest
    {
        public IObservable<Tuple<TSource, TOther>> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> second)
        {
            return source.CombineLatest(second, (xs, ys) => Tuple.Create(xs, ys));
        }

        public IObservable<Tuple<TSource, TSource, TSource>> Process<TSource>(
            IObservable<TSource> source,
            IObservable<TSource> source2,
            IObservable<TSource> source3)
        {
            return source.CombineLatest(source2, source3, (s1, s2, s3) => Tuple.Create(s1, s2, s3));
        }

        public IObservable<Tuple<TSource, TSource, TSource, TSource>> Process<TSource>(
            IObservable<TSource> source,
            IObservable<TSource> source2,
            IObservable<TSource> source3,
            IObservable<TSource> source4)
        {
            return source.CombineLatest(source2, source3, source4, (s1, s2, s3, s4) => Tuple.Create(s1, s2, s3, s4));
        }

        public IObservable<Tuple<TSource, TSource, TSource, TSource, TSource>> Process<TSource>(
            IObservable<TSource> source,
            IObservable<TSource> source2,
            IObservable<TSource> source3,
            IObservable<TSource> source4,
            IObservable<TSource> source5)
        {
            return source.CombineLatest(source2, source3, source4, source5,
                (s1, s2, s3, s4, s5) => Tuple.Create(s1, s2, s3, s4, s5));
        }

        public IObservable<Tuple<TSource, TSource, TSource, TSource, TSource, TSource>> Process<TSource>(
            IObservable<TSource> source,
            IObservable<TSource> source2,
            IObservable<TSource> source3,
            IObservable<TSource> source4,
            IObservable<TSource> source5,
            IObservable<TSource> source6)
        {
            return source.CombineLatest(source2, source3, source4, source5, source6,
                (s1, s2, s3, s4, s5, s6) => Tuple.Create(s1, s2, s3, s4, s5, s6));
        }

        public IObservable<Tuple<TSource, TSource, TSource, TSource, TSource, TSource, TSource>> Process<TSource>(
            IObservable<TSource> source,
            IObservable<TSource> source2,
            IObservable<TSource> source3,
            IObservable<TSource> source4,
            IObservable<TSource> source5,
            IObservable<TSource> source6,
            IObservable<TSource> source7)
        {
            return source.CombineLatest(source2, source3, source4, source5, source6, source7,
                (s1, s2, s3, s4, s5, s6, s7) => Tuple.Create(s1, s2, s3, s4, s5, s6, s7));
        }

        public IObservable<IList<TSource>> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.CombineLatest(sources);
        }
    }
}
