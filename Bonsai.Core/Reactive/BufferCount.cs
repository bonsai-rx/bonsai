using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that projects each element of the sequence into zero or more
    /// buffers based on element count information.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into zero or more buffers based on element count information.")]
    public class BufferCount
    {
        /// <summary>
        /// Gets or sets the number of elements in each buffer.
        /// </summary>
        [Description("The number of elements in each buffer.")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between the creation of
        /// consecutive buffers.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the operator will generate consecutive
        /// non-overlapping buffers.
        /// </remarks>
        [Description("The number of elements to skip between the creation of consecutive buffers.")]
        public int? Skip { get; set; }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers
        /// based on element count information.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to produce buffers over.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public IObservable<IList<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            var skip = Skip;
            if (skip.HasValue) return source.Buffer(Count, skip.Value);
            else return source.Buffer(Count);
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="BufferCount"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(BufferCount))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class Buffer : BufferCount
    {
    }
}
