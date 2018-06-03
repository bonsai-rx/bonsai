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
    /// Represents a combinator that returns a specified number of contiguous elements
    /// from the end of an observable sequence.
    /// </summary>
    [DefaultProperty("Count")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the specified number of contiguous elements from the end of the sequence.")]
    public class TakeLast : Combinator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeLast"/> class.
        /// </summary>
        public TakeLast()
        {
            Count = 1;
        }

        /// <summary>
        /// Gets or sets the number of elements to take from the end of the sequence.
        /// </summary>
        [Description("The number of elements to take from the end of the sequence.")]
        public int Count { get; set; }

        /// <summary>
        /// Returns a specified number of contiguous elements from the end of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence to take elements from.</param>
        /// <returns>
        /// An observable sequence containing the specified number of elements from the
        /// end of the source sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.TakeLast(Count);
        }
    }
}
