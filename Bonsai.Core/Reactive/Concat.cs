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
    /// <summary>
    /// Represents a combinator that concatenates any number of observable sequences as long as the
    /// previous sequence terminated successfully.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Concatenates any number of observable sequences as long as the previous sequence terminated successfully.")]
    public class Concat
    {
        /// <summary>
        /// Concatenates the second observable sequence to the first observable sequence upon
        /// successful termination of the first.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the first sequence.</typeparam>
        /// <param name="first">The first observable sequence.</param>
        /// <param name="second">The second observable sequence.</param>
        /// <returns>
        /// An observable sequence that contains the elements of the first sequence,
        /// followed by those of the second sequence.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> first, IObservable<TSource> second)
        {
            return first.Concat(second);
        }

        /// <summary>
        /// Concatenates all of the specified observable sequences, as long as the previous
        /// observable sequence terminated successfully.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="first">The first observable sequence.</param>
        /// <param name="second">The second observable sequence.</param>
        /// <param name="remainder">The remaining observable sequences to concatenate.</param>
        /// <returns>
        /// An observable sequence that contains the elements of each given sequence,
        /// in sequential order.
        /// </returns>
        public IObservable<TSource> Process<TSource>(
            IObservable<TSource> first,
            IObservable<TSource> second,
            params IObservable<TSource>[] remainder)
        {
            return Observable.Concat(EnumerableEx.Concat(first, second, remainder));
        }

        /// <summary>
        /// Concatenates all inner observable sequences, as long as the previous observable
        /// sequence terminated successfully.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="sources">The observable sequence of inner observable sequences.</param>
        /// <returns>
        /// An observable sequence that contains the elements of each observed inner
        /// sequence, in sequential order.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<IObservable<TSource>> sources)
        {
            return sources.Concat();
        }

        /// <summary>
        /// Concatenates all inner enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="sources">The observable sequence of inner enumerable sequences.</param>
        /// <returns>
        /// An observable sequence that contains the elements of each inner enumerable
        /// sequence, in sequential order.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<IEnumerable<TSource>> sources)
        {
            return sources.SelectMany(xs => xs);
        }
    }
}
