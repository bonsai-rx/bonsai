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
    /// Represents a combinator that projects each element of the sequence into consecutive
    /// non-overlapping buffers.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into consecutive non-overlapping buffers.")]
    public class TriggeredBuffer
    {
        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TBufferBoundary">
        /// The type of the elements in the sequence indicating buffer boundary events.
        /// </typeparam>
        /// <param name="source">The source sequence to produce buffers over.</param>
        /// <param name="bufferBoundaries">
        /// The sequence of buffer boundary markers. The current buffer is closed and a new
        /// buffer is opened upon receiving a boundary marker.
        /// </param>
        /// <returns>An observable sequence of buffers.</returns>
        public IObservable<IList<TSource>> Process<TSource, TBufferBoundary>(IObservable<TSource> source, IObservable<TBufferBoundary> bufferBoundaries)
        {
            return source.Buffer(bufferBoundaries);
        }
    }
}
