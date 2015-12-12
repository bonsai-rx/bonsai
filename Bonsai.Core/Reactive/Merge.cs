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
    /// <summary>
    /// Represents a combinator that merges any number of observable sequences into a
    /// single observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Merges any number of obervable sequences into a single observable sequence.")]
    public class Merge
    {
        /// <summary>
        /// Merges elements from two observable sequences into a single observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the first sequence.</typeparam>
        /// <param name="first">The first observable sequence.</param>
        /// <param name="second">The second observable sequence.</param>
        /// <returns>The observable sequence that merges the elements of the two sequences.</returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> first, IObservable<TSource> second)
        {
            return first.Merge(second);
        }

        /// <summary>
        /// Merges elements from all inner observable sequences into a single observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="sources">The observable sequence of inner observable sequences.</param>
        /// <returns>The observable sequence that merges the elements of the inner sequences.</returns>
        public IObservable<TSource> Process<TSource>(IObservable<IObservable<TSource>> sources)
        {
            return sources.Merge();
        }

        /// <summary>
        /// Merges elements from all inner grouped observable sequences into a single observable sequence.
        /// </summary>
        /// <typeparam name="TKey">The type of the key shared by all elements in the group.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each group.</typeparam>
        /// <param name="sources">The observable sequence of inner observable sequences.</param>
        /// <returns>The observable sequence that merges the elements of the inner sequences.</returns>
        public IObservable<TElement> Process<TKey, TElement>(IObservable<IGroupedObservable<TKey, TElement>> sources)
        {
            return sources.Merge();
        }

        /// <summary>
        /// Merges elements from the specified observable sequences into a single observable
        /// sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="sources">The observable sequences to merge.</param>
        /// <returns>The observable sequence that merges the elements of the observable sequences.</returns>
        public IObservable<TSource> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.Merge(sources);
        }
    }
}
