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
    /// <summary>
    /// Represents a combinator that combines values from the source sequences whenever
    /// all of the sequences have produced an element.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Combines values from the source sequences whenever all of the sequences have produced an element.")]
    public class Zip
    {
        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a list with
        /// the elements of the observable sequences whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource1">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TSource2">The type of the elements in the second source sequence.</typeparam>
        /// <param name="source1">The first observable source.</param>
        /// <param name="source2">The second observable source.</param>
        /// <returns>
        /// An observable sequence containing the result of combining elements of the sources into tuples.
        /// </returns>
        public IObservable<Tuple<TSource1, TSource2>> Process<TSource1, TSource2>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2)
        {
            return source1.Zip(source2, (xs, ys) => Tuple.Create(xs, ys));
        }

        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a list with
        /// the elements of the observable sequences whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource1">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TSource2">The type of the elements in the second source sequence.</typeparam>
        /// <typeparam name="TSource3">The type of the elements in the third source sequence.</typeparam>
        /// <param name="source1">The first observable source.</param>
        /// <param name="source2">The second observable source.</param>
        /// <param name="source3">The third observable source.</param>
        /// <returns>
        /// An observable sequence containing the result of combining elements of the sources into tuples.
        /// </returns>
        public IObservable<Tuple<TSource1, TSource2, TSource3>> Process<TSource1, TSource2, TSource3>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2,
            IObservable<TSource3> source3)
        {
            return source1.Zip(source2, source3, (s1, s2, s3) => Tuple.Create(s1, s2, s3));
        }

        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a list with
        /// the elements of the observable sequences whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource1">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TSource2">The type of the elements in the second source sequence.</typeparam>
        /// <typeparam name="TSource3">The type of the elements in the third source sequence.</typeparam>
        /// <typeparam name="TSource4">The type of the elements in the fourth source sequence.</typeparam>
        /// <param name="source1">The first observable source.</param>
        /// <param name="source2">The second observable source.</param>
        /// <param name="source3">The third observable source.</param>
        /// <param name="source4">The fourth observable source.</param>
        /// <returns>
        /// An observable sequence containing the result of combining elements of the sources into tuples.
        /// </returns>
        public IObservable<Tuple<TSource1, TSource2, TSource3, TSource4>> Process<TSource1, TSource2, TSource3, TSource4>(
            IObservable<TSource1> source1,
            IObservable<TSource2> source2,
            IObservable<TSource3> source3,
            IObservable<TSource4> source4)
        {
            return source1.Zip(source2, source3, source4, (s1, s2, s3, s4) => Tuple.Create(s1, s2, s3, s4));
        }

        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a list with
        /// the elements of the observable sequences whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource1">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TSource2">The type of the elements in the second source sequence.</typeparam>
        /// <typeparam name="TSource3">The type of the elements in the third source sequence.</typeparam>
        /// <typeparam name="TSource4">The type of the elements in the fourth source sequence.</typeparam>
        /// <typeparam name="TSource5">The type of the elements in the fifth source sequence.</typeparam>
        /// <param name="source1">The first observable source.</param>
        /// <param name="source2">The second observable source.</param>
        /// <param name="source3">The third observable source.</param>
        /// <param name="source4">The fourth observable source.</param>
        /// <param name="source5">The fifth observable source.</param>
        /// <returns>
        /// An observable sequence containing the result of combining elements of the sources into tuples.
        /// </returns>
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

        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a list with
        /// the elements of the observable sequences whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource1">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TSource2">The type of the elements in the second source sequence.</typeparam>
        /// <typeparam name="TSource3">The type of the elements in the third source sequence.</typeparam>
        /// <typeparam name="TSource4">The type of the elements in the fourth source sequence.</typeparam>
        /// <typeparam name="TSource5">The type of the elements in the fifth source sequence.</typeparam>
        /// <typeparam name="TSource6">The type of the elements in the sixth source sequence.</typeparam>
        /// <param name="source1">The first observable source.</param>
        /// <param name="source2">The second observable source.</param>
        /// <param name="source3">The third observable source.</param>
        /// <param name="source4">The fourth observable source.</param>
        /// <param name="source5">The fifth observable source.</param>
        /// <param name="source6">The sixth observable source.</param>
        /// <returns>
        /// An observable sequence containing the result of combining elements of the sources into tuples.
        /// </returns>
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

        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a list with
        /// the elements of the observable sequences whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource1">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TSource2">The type of the elements in the second source sequence.</typeparam>
        /// <typeparam name="TSource3">The type of the elements in the third source sequence.</typeparam>
        /// <typeparam name="TSource4">The type of the elements in the fourth source sequence.</typeparam>
        /// <typeparam name="TSource5">The type of the elements in the fifth source sequence.</typeparam>
        /// <typeparam name="TSource6">The type of the elements in the sixth source sequence.</typeparam>
        /// <typeparam name="TSource7">The type of the elements in the seventh source sequence.</typeparam>
        /// <param name="source1">The first observable source.</param>
        /// <param name="source2">The second observable source.</param>
        /// <param name="source3">The third observable source.</param>
        /// <param name="source4">The fourth observable source.</param>
        /// <param name="source5">The fifth observable source.</param>
        /// <param name="source6">The sixth observable source.</param>
        /// <param name="source7">The seventh observable source.</param>
        /// <returns>
        /// An observable sequence containing the result of combining elements of the sources into tuples.
        /// </returns>
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

        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a list with
        /// the elements of the observable sequences whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="first">The first observable sequence.</param>
        /// <param name="second">The second observable sequence.</param>
        /// <param name="remainder">The remaining observable sequences to combine.</param>
        /// <returns>
        /// An observable sequence containing the result of combining the elements of the
        /// sources into lists.
        /// </returns>
        public IObservable<IList<TSource>> Process<TSource>(
            IObservable<TSource> first,
            IObservable<TSource> second,
            params IObservable<TSource>[] remainder)
        {
            return Observable.Zip(EnumerableEx.Concat(first, second, remainder));
        }

        /// <summary>
        /// Merges elements from all inner observable sequences into one observable sequence by emitting
        /// a list with the elements of each sequence whenever all of the sequences have produced
        /// a new element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="sources">The observable sequence of inner observable sequences.</param>
        /// <returns>
        /// An observable sequence containing the result of combining the elements of the
        /// inner sequences into lists.
        /// </returns>
        public IObservable<IList<TSource>> Process<TSource>(IObservable<IObservable<TSource>> sources)
        {
            return sources.ToArray().SelectMany(xs => xs.Zip());
        }
    }
}
