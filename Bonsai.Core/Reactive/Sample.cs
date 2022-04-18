using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that samples elements from an observable sequence
    /// whenever the second sequence emits a notification.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Samples elements from the first sequence whenever the second sequence emits a notification.")]
    public class Sample : BinaryCombinator
    {
        /// <summary>
        /// Samples elements from the first sequence whenever the second sequence
        /// emits a notification.
        /// </summary>
        /// <remarks>
        /// Upon each sampling notification, the latest element (if any) emitted by
        /// the <paramref name="source"/> sequence during the last sampling interval
        /// is sent to the resulting sequence.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to sample.</param>
        /// <param name="other">The sequence of sampling notifications.</param>
        /// <returns>The sampled observable sequence.</returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Sample(other);
        }
    }
}
