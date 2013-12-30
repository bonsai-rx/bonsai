using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that subscribes to an observable sequence only after the second
    /// sequence produces an element.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Subscribes to the first observable sequence only after the second sequence produces an element.")]
    public class SubscribeWhen : BinaryCombinator
    {
        /// <summary>
        /// Subscribes to an observable sequence only after the second sequence
        /// produces an element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the other sequence that indicates the start of
        /// subscription to the first sequence.
        /// </typeparam>
        /// <param name="source">The sequence to subscribe to.</param>
        /// <param name="other">
        /// The observable sequence that initiates subscription to the source sequence.
        /// </param>
        /// <returns>
        /// An observable sequence that propagates elements of the source sequence but where
        /// subscription is delayed until a second sequence produces an element.
        /// </returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return other.Take(1).SelectMany(x => source);
        }
    }
}
