using System;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that subscribes to the first sequence only after the second
    /// sequence emits a notification.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Subscribes to the first sequence only after the second sequence emits a notification.")]
    public class SubscribeWhen : BinaryCombinator
    {
        /// <summary>
        /// Subscribes to an observable sequence only after the second sequence
        /// produces an element.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The sequence to subscribe to.</param>
        /// <param name="other">
        /// The observable sequence indicating when to subscribe to the
        /// <paramref name="source"/> sequence.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where subscription is delayed until the <paramref name="other"/>
        /// sequence emits a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return other.Take(1).SelectMany(x => source);
        }
    }
}
