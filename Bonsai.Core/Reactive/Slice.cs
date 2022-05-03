using System;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that extracts a range of elements from an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Step))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Extracts a range of elements from an observable sequence.")]
    public class Slice : Combinator
    {
        /// <summary>
        /// Gets or sets the element index at which the slice begins.
        /// </summary>
        [Description("The element index at which the slice begins.")]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between slice elements.
        /// </summary>
        [Description("The number of elements to skip between slice elements.")]
        public int Step { get; set; } = 1;

        /// <summary>
        /// Gets or sets the element index at which the slice ends. If no value
        /// is specified, elements will be taken until the end of the sequence.
        /// </summary>
        [Description("The element index at which the slice ends. If no value is specified, elements will be taken until the end of the sequence.")]
        public int? Stop { get; set; }

        /// <summary>
        /// Extracts a range of elements from an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to slice.</param>
        /// <returns>The sliced sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                int i = 0;
                return source.Where(xs =>
                {
                    var index = i++;
                    return index >= Start && (!Stop.HasValue || index < Stop) && (index - Start) % Step == 0;
                });
            });
        }
    }
}
