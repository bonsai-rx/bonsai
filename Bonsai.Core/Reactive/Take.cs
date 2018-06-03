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
    /// Represents a combinator that returns the specified number of contiguous elements
    /// from the start of an observable sequence.
    /// </summary>
    [DefaultProperty("Count")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the specified number of contiguous elements from the start of the sequence.")]
    public class Take : Combinator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Take"/> class.
        /// </summary>
        public Take()
        {
            Count = 1;
        }

        /// <summary>
        /// Gets or sets the number of elements to take.
        /// </summary>
        [Description("The number of elements to take.")]
        public int Count { get; set; }

        /// <summary>
        /// Returns the specified number of contiguous elements from the start of an
        /// observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence to take elements from.</param>
        /// <returns>
        /// An observable sequence that contains the specified number of elements from
        /// the start of the input sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Take(Count);
        }
    }
}
