using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns the elements from the first sequence
    /// only after the second sequence emits a notification.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns elements from the first sequence only after the second sequence emits a notification.")]
    public class SkipUntil : BinaryCombinator
    {
        /// <summary>
        /// Returns the elements from an observable sequence only after the second
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to propagate elements for.</param>
        /// <param name="other">
        /// The observable sequence indicating the time at which to start taking
        /// elements from the <paramref name="source"/> sequence.
        /// </param>
        /// <returns>
        /// An observable sequence containing the elements of the <paramref name="source"/>
        /// sequence emitted after the <paramref name="other"/> sequence emits
        /// a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.SkipUntil(other);
        }
    }
}
