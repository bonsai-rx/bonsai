using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that projects each element of an observable sequence into zero
    /// or more buffers aligned on an external trigger.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more buffers aligned on when the second sequence produces an element.")]
    public class TriggeredBuffer
    {
        /// <summary>
        /// Gets or sets the time length of each buffer. If it is not specified, the buffer will have
        /// its length set by either a maximum number of elements or an external trigger boundary.
        /// </summary>
        [XmlIgnore]
        [Description("The optional time length of each buffer.")]
        public TimeSpan? TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of elements in each buffer. If it is not specified, the
        /// buffer will have its length set by the optional time span or by an external trigger boundary.
        /// </summary>
        [Description("The optional maximum number of elements in each buffer.")]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of buffer time span.
        /// </summary>
        [Browsable(false)]
        [XmlElement("TimeSpan")]
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
        /// Projects each element of an observable sequence into zero or more buffers.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TBufferOpening">
        /// The type of the elements in the sequence indicating buffer opening events.
        /// </typeparam>
        /// <param name="source">The source sequence to produce buffers over.</param>
        /// <param name="bufferOpenings">
        /// The sequence of buffer opening events. If no maximum length is specified, the current
        /// buffer is closed and a new buffer is opened upon receiving a buffer opening event.
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
}
