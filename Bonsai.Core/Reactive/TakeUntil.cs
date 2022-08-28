using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns elements from the first sequence only until
    /// the second sequence emits a notification.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns elements from the first sequence only until the second sequence emits a notification.")]
    public class TakeUntil
    {
        /// <summary>
        /// Returns elements from an observable sequence only until the second sequence
        /// emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The sequence to take elements from.</param>
        /// <param name="other">
        /// The observable sequence indicating the time at which to stop taking elements
        /// from the <paramref name="source"/> sequence.
        /// </param>
        /// <returns>
        /// An observable sequence containing the elements of the <paramref name="source"/>
        /// sequence emitted until the <paramref name="other"/> sequence emits
        /// a notification.
        /// </returns>
        public IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.TakeUntil(other);
        }
    }
}
