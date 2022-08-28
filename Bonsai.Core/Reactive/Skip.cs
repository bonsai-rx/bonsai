using System;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that bypasses the specified number of elements at the start
    /// of an observable sequence and returns the remaining elements.
    /// </summary>
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Bypasses the specified number of elements at the start of the sequence and returns the remaining elements.")]
    public class Skip : Combinator
    {
        /// <summary>
        /// Gets or sets the number of elements to skip.
        /// </summary>
        [Description("The number of elements to skip.")]
        public int Count { get; set; } = 1;

        /// <summary>
        /// Bypasses the specified number of elements at the start of an observable
        /// sequence and returns the remaining elements.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The sequence to skip elements from.</param>
        /// <returns>
        /// An observable sequence that contains the elements that occur after the
        /// skipped elements in the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Skip(Count);
        }
    }
}
