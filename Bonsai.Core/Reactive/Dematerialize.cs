using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that dematerializes the explicit notification values of an observable
    /// sequence as implicit notifications.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Dematerializes the explicit notification values of an observable sequence as implicit notifications.")]
    public class Dematerialize
    {
        /// <summary>
        /// Dematerializes the explicit notification values of an observable sequence as implicit
        /// notifications.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements materialized in the source sequence notification objects.
        /// </typeparam>
        /// <param name="source">
        /// An observable sequence containing explicit notification values which have
        /// to be turned into implicit notifications.
        /// </param>
        /// <returns>
        /// An observable sequence exhibiting the behavior corresponding to the notification
        /// values of the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<Notification<TSource>> source)
        {
            return source.Dematerialize();
        }
    }
}
