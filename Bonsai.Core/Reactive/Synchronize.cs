using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that synchronizes the observable sequence
    /// to ensure that observer notifications cannot be delivered concurrently.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Synchronizes the observable sequence to ensure that observer notifications cannot be delivered concurrently.")]
    public class Synchronize : Combinator
    {
        /// <summary>
        /// Synchronizes the observable sequence to ensure that observer notifications
        /// cannot be delivered concurrently.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to synchronize.</param>
        /// <returns>
        /// The source sequence whose outgoing calls to observers are synchronized.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Synchronize();
        }
    }
}
