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
    /// Represents a combinator that materializes the implicit notifications of an observable
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
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence to get notification values for.</param>
        /// <returns>
        /// An observable sequence containing the materialized notification values from
        /// the source sequence.
        /// </returns>
        public IObservable<Notification<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Materialize();
        }
    }
}
