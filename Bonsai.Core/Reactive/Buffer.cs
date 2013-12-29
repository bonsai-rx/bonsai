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
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into zero or more buffers based on element count information.")]
    public class Buffer
    {
        /// <summary>
        /// Gets or sets the length of each buffer.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between the creation of
        /// consecutive buffers.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Projects each element of the <paramref name="source"/> sequence into zero or more
        /// buffers based on element count information.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to produce buffers over.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public IObservable<IList<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Buffer(Count, Skip);
        }
    }
}
