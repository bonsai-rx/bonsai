using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;
using System.Collections.Generic;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that projects each element of the sequence into zero or more
    /// buffers based on timing information.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(TimeSpan))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into zero or more buffers based on timing information.")]
    public class BufferTime
    {
        /// <summary>
        /// Gets or sets the length of each buffer.
        /// </summary>
        [XmlIgnore]
        [Description("The length of each buffer.")]
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the interval between creation of consecutive buffers.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the operator will generate consecutive
        /// non-overlapping buffers.
        /// </remarks>
        [XmlIgnore]
        [Description("The interval between creation of consecutive buffers.")]
        public TimeSpan? TimeShift { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the buffer time span for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeSpan))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Gets or sets an XML representation of the buffer time shift for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeShift))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string TimeShiftXml
        {
            get
            {
                var timeShift = TimeShift;
                if (timeShift.HasValue) return XmlConvert.ToString(timeShift.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) TimeShift = XmlConvert.ToTimeSpan(value);
                else TimeShift = null;
            }
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers
        /// based on timing information.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to produce buffers over.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public IObservable<IList<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            var timeShift = TimeShift;
            if (timeShift.HasValue) return source.Buffer(TimeSpan, timeShift.Value, HighResolutionScheduler.Default);
            else return source.Buffer(TimeSpan, HighResolutionScheduler.Default);
        }
    }
}
