using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that returns the last element of an observable sequence.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the last element of an observable sequence.")]
    public class Last : Combinator
    {
        /// <summary>
        /// Returns the last element of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence to take the last element from.</param>
        /// <returns>
        /// An observable sequence with a single element that contains the last element
        /// of the observable sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.LastAsync();
        }
    }
}
