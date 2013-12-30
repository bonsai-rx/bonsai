using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that removes timestamp information from the elements of
    /// an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Removes timestamp information from the elements of the sequence.")]
    public class RemoveTimestamp
    {
        /// <summary>
        /// Removes timestamp information from the elements of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the timestamped source sequence.</typeparam>
        /// <param name="source">The timestamped source sequence on which to remove timestamps.</param>
        /// <returns>An observable sequence with timestamp information removed.</returns>
        public IObservable<TSource> Process<TSource>(IObservable<Timestamped<TSource>> source)
        {
            return source.Select(xs => xs.Value);
        }
    }
}
