using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that dematerializes the explicit notification values of an observable
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
        /// An observable sequence exhibiting the behavior corresponding to the source
        /// sequence's notification values.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<Notification<TSource>> source)
        {
            return source.Dematerialize();
        }
    }
}
