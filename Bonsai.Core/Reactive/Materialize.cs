using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that materializes the implicit notifications of an observable
    /// sequence as explicit notification values.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Materializes the implicit notifications of a sequence as explicit notification values.")]
    public class Materialize
    {
        /// <summary>
        /// Materializes the implicit notifications of an observable sequence as explicit
        /// notification values.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">An observable sequence to get notification values for.</param>
        /// <returns>
        /// An observable sequence containing the materialized notification values from
        /// the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Notification<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Materialize();
        }
    }
}
