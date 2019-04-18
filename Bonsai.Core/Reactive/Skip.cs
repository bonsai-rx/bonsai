using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that bypasses the specified number of elements at the start
    /// of an observable sequence and returns the remaining elements.
    /// </summary>
    [DefaultProperty("Count")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Bypasses the specified number of contiguous elements at the start of the sequence.")]
    public class Skip : Combinator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skip"/> class.
        /// </summary>
        public Skip()
        {
            Count = 1;
        }

        /// <summary>
        /// Gets or sets the number of elements to skip.
        /// </summary>
        [Description("The number of elements to skip.")]
        public int Count { get; set; }

        /// <summary>
        /// Bypasses the specified number of elements at the start of an observable sequence
        /// and returns the remaining elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence to take elements from.</param>
        /// <returns>
        /// An observable sequence that contains the elements that occur after the specified
        /// index in the input sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Skip(Count);
        }
    }
}
