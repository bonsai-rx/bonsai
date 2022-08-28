using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that bypasses the specified number of elements at the end
    /// of an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Bypasses the specified number of elements at the end of the sequence.")]
    public class SkipLast : Combinator
    {
        /// <summary>
        /// Gets or sets the number of elements to skip at the end of the sequence.
        /// </summary>
        [Description("The number of elements to skip at the end of the sequence.")]
        public int Count { get; set; } = 1;

        /// <summary>
        /// Bypasses the specified number of elements at the end of an observable
        /// sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The sequence to skip elements from.</param>
        /// <returns>
        /// An observable sequence containing the elements in the <paramref name="source"/>
        /// sequence excluding the ones which are bypassed at the end.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.SkipLast(Count);
        }
    }
}
