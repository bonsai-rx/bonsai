using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that allows a single element from the first sequence
    /// to pass through every time a second sequence emits a notification.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Allows a single element from the first sequence to pass through every time a second sequence emits a notification.")]
    public class Gate
    {
        /// <summary>
        /// Gets or sets a value specifying the maximum time the gate stays open.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the gate stays open indefinitely until an element
        /// arrives. If a maximum due time is specified, however, then if an element
        /// from the first sequence arrives after this interval elapses, that element
        /// will not be allowed through and will be dropped from the result sequence.
        /// </remarks>
        [XmlIgnore]
        [Description("The maximum time the gate stays open.")]
        public TimeSpan? DueTime { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the gate due time for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(DueTime))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DueTimeXml
        {
            get
            {
                var timeSpan = DueTime;
                if (timeSpan.HasValue) return XmlConvert.ToString(timeSpan.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) DueTime = XmlConvert.ToTimeSpan(value);
                else DueTime = null;
            }
        }

        /// <summary>
        /// Allows a single element from an observable sequence to pass through every
        /// time a second sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TGateOpening">
        /// The type of the elements in the <paramref name="gateOpenings"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to filter.</param>
        /// <param name="gateOpenings">The sequence of gate opening events.</param>
        /// <returns>
        /// The filtered observable sequence. Every time the <paramref name="gateOpenings"/>
        /// sequence produces a notification, the next element from the
        /// <paramref name="source"/> sequence will be allowed through.
        /// </returns>
        public IObservable<TSource> Process<TSource, TGateOpening>(IObservable<TSource> source, IObservable<TGateOpening> gateOpenings)
        {
            var dueTime = DueTime;
            return dueTime.HasValue ? source.Gate(gateOpenings, dueTime.Value) : source.Gate(gateOpenings);
        }
    }
}
