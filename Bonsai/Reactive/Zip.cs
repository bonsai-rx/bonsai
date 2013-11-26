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
    [Description("Combines values from the input sequences whenever all of the sequences have produced an element.")]
    public class Zip
    {
        public IObservable<Tuple<TSource1, TSource2>> Process<TSource1, TSource2>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2)
        {
            return source1.Zip(source2, (xs, ys) => Tuple.Create(xs, ys));
        }

        public IObservable<Tuple<TSource1, TSource2, TSource3>> Process<TSource1, TSource2, TSource3>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2,
            IObservable<TSource3> source3)
        {
            return source1.Zip(source2, source3, (s1, s2, s3) => Tuple.Create(s1, s2, s3));
        }

        public IObservable<Tuple<TSource1, TSource2, TSource3, TSource4>> Process<TSource1, TSource2, TSource3, TSource4>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2,
            IObservable<TSource3> source3,
            IObservable<TSource4> source4)
        {
            return source1.Zip(source2, source3, source4, (s1, s2, s3, s4) => Tuple.Create(s1, s2, s3, s4));
        }

        public IObservable<Tuple<TSource1, TSource2, TSource3, TSource4, TSource5>>
            Process<TSource1, TSource2, TSource3, TSource4, TSource5>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2,
            IObservable<TSource3> source3,
            IObservable<TSource4> source4,
            IObservable<TSource5> source5)
        {
            return source1.Zip(source2, source3, source4, source5,
                (s1, s2, s3, s4, s5) => Tuple.Create(s1, s2, s3, s4, s5));
        }

        public IObservable<Tuple<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>> Process<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2,
            IObservable<TSource3> source3,
            IObservable<TSource4> source4,
            IObservable<TSource5> source5,
            IObservable<TSource6> source6)
        {
            return source1.Zip(source2, source3, source4, source5, source6,
                (s1, s2, s3, s4, s5, s6) => Tuple.Create(s1, s2, s3, s4, s5, s6));
        }

        public IObservable<Tuple<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>>
            Process<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2,
            IObservable<TSource3> source3,
            IObservable<TSource4> source4,
            IObservable<TSource5> source5,
            IObservable<TSource6> source6,
            IObservable<TSource7> source7)
        {
            return source1.Zip(source2, source3, source4, source5, source6, source7,
                (s1, s2, s3, s4, s5, s6, s7) => Tuple.Create(s1, s2, s3, s4, s5, s6, s7));
        }

        public IObservable<IList<TSource>> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.Zip(sources);
        }
    }
}
