using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that takes the next element from the first sequence
    /// whenever the second sequence emits a notification.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Takes the next element from the first sequence whenever the second sequence emits a notification.")]
    public class Gate
    {
        /// <summary>
        /// Takes the next element from the first observable sequence whenever
        /// the second sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to take elements from.</param>
        /// <param name="other">
        /// The sequence of gate events. Every time this sequence produces a notification,
        /// the next element from the <paramref name="source"/> sequence is taken.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Gate(other);
        }
    }
}
