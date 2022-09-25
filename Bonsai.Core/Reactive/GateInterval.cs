using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;
using System.Reactive.Linq;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that allows a single element from the first sequence
    /// to pass through every time the specified time interval elapses.
    /// </summary>
    [DefaultProperty(nameof(Interval))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Allows a single element from the first sequence to pass through every time the specified interval elapses.")]
    public class GateInterval : Combinator
    {
        /// <summary>
        /// Gets or sets the period after which a new element from the sequence is
        /// allowed to pass through the gate.
        /// </summary>
        [XmlIgnore]
        [Description("The period after which a new element from the sequence is allowed to pass through the gate.")]
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the interval for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Interval))]
        public string IntervalXml
        {
            get { return XmlConvert.ToString(Interval); }
            set { Interval = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Gets or sets a value specifying the maximum time the gate stays open.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the gate stays open indefinitely until an element
        /// arrives. If a maximum time span is specified, however, then if an element
        /// from the first sequence arrives after this interval elapses, it will be
        /// dropped from the result sequence.
        /// </remarks>
        [XmlIgnore]
        [Description("The maximum time the gate stays open.")]
        public TimeSpan? TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the gate time span for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeSpan))]
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
        /// Allows a single element from an observable sequence to pass through every
        /// time the specified time interval elapses.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to filter.</param>
        /// <returns>
        /// The filtered observable sequence. Every time the specified time interval
        /// elapses, the next element from the <paramref name="source"/> sequence
        /// will be allowed through.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var timeSpan = TimeSpan;
            var scheduler = HighResolutionScheduler.Default;
            var interval = Observable.Timer(Interval, scheduler);
            return timeSpan.HasValue
                ? source.Gate(interval, timeSpan.Value, scheduler)
                : source.Gate(interval);
        }
    }
}
