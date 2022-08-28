using System;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns the specified number of contiguous elements
    /// from the start of an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the specified number of contiguous elements from the start of the sequence.")]
    public class Take : Combinator
    {
        /// <summary>
        /// Gets or sets the number of elements to take.
        /// </summary>
        [Description("The number of elements to take.")]
        public int Count { get; set; } = 1;

        /// <summary>
        /// Returns the specified number of contiguous elements from the start of an
        /// observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The sequence to take elements from.</param>
        /// <returns>
        /// An observable sequence that contains the specified number of elements from
        /// the start of the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Take(Count);
        }
    }
}
