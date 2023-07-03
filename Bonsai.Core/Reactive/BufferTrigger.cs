using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that projects each element of an observable sequence into zero
    /// or more buffers created when the second sequence emits a notification.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into zero or more buffers created when the second sequence emits a notification.")]
    public class BufferTrigger
    {
        /// <summary>
        /// Gets or sets the time length of each buffer.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the buffer will have its length specified by either a
        /// maximum number of elements, or the boundary indicated by a notification
        /// from the second sequence.
        /// </remarks>
        [XmlIgnore]
        [Description("The time length of each buffer.")]
        public TimeSpan? TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of elements in each buffer.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the buffer will have its length specified by either a
        /// maximum time span, or the boundary indicated by a notification from the second
        /// sequence.
        /// </remarks>
        [Description("The maximum number of elements in each buffer.")]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the buffer time span for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeSpan))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string TimeSpanXml
        {
            get
            {
                var timeSpan = TimeSpan;
                if (timeSpan.HasValue) return XmlConvert.ToString(timeSpan.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) TimeSpan = XmlConvert.ToTimeSpan(value);
                else TimeSpan = null;
            }
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers
        /// created when a second sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TBufferOpening">
        /// The type of the elements in the <paramref name="bufferOpenings"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to produce buffers over.</param>
        /// <param name="bufferOpenings">
        /// The sequence of buffer openings. If no maximum length is specified, the current
        /// buffer is closed and a new buffer is opened upon receiving a notification from
        /// this sequence.
        /// </param>
        /// <returns>An observable sequence of buffers.</returns>
        public IObservable<IList<TSource>> Process<TSource, TBufferOpening>(IObservable<TSource> source, IObservable<TBufferOpening> bufferOpenings)
        {
            var count = Count;
            var timeSpan = TimeSpan;
            if (timeSpan.HasValue && count.HasValue)
            {
                return source.Publish(ps => ps.Buffer(bufferOpenings, x => Observable.Merge(
                    Observable.Timer(timeSpan.Value, HighResolutionScheduler.Default),
                    ps.Take(count.Value).LongCount())));
            }
            else if (timeSpan.HasValue) return source.Buffer(bufferOpenings, x => Observable.Timer(timeSpan.Value, HighResolutionScheduler.Default));
            else if (count.HasValue) return source.Publish(ps => ps.Buffer(bufferOpenings, x => ps.Take(count.Value).Count()));
            else return source.Buffer(bufferOpenings);
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="BufferTrigger"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(BufferTrigger))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TriggeredBuffer : BufferTrigger
    {
    }
}
