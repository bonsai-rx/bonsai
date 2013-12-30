using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that returns elements from an observable sequence only until
    /// the second sequence produces an element.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns elements from the first sequence only until the second sequence produces a value.")]
    public class TakeUntil : BinaryCombinator
    {
        /// <summary>
        /// Returns elements from an observable sequence only until the second sequence
        /// produces an element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the other sequence that indicates the end of
        /// take behavior.
        /// </typeparam>
        /// <param name="source">The sequence to take elements from.</param>
        /// <param name="other">
        /// The observable sequence that terminates propagation of elements of the source sequence.
        /// </param>
        /// <returns>
        /// An observable sequence containing the elements of the source sequence up
        /// to the point the other sequence interrupted further propagation.
        /// </returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.TakeUntil(other);
        }
    }
}
