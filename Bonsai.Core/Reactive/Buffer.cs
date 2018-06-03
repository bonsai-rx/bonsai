using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that projects each element of the sequence into zero or more
    /// buffers based on element count information.
    /// </summary>
    [Combinator]
    [DefaultProperty("Count")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into zero or more buffers based on element count information.")]
    public class Buffer
    {
        /// <summary>
        /// Gets or sets the length of each buffer.
        /// </summary>
        [Description("The number of elements in each buffer.")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between the creation of consecutive buffers.
        /// If it is not specified, <see cref="Skip"/> will be equal to <see cref="Count"/> in order
        /// to generate consecutive non-overlapping buffers.
        /// </summary>
        [Description("The optional number of elements to skip between the creation of each buffer.")]
        public int? Skip { get; set; }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers
        /// based on element count information.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to produce buffers over.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public IObservable<IList<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            var skip = Skip;
            if (skip.HasValue) return source.Buffer(Count, skip.Value);
            else return source.Buffer(Count);
        }
    }
}
