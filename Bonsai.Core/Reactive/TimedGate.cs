using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that takes the next element from the sequence if this
    /// element is produced within a specified time interval after the gate sequence
    /// emits a notification.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(TimeSpan))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Takes the next element from the sequence if this element is produced within a specified interval after the gate sequence emits a notification.")]
    public class TimedGate
    {
        /// <summary>
        /// Gets or sets the maximum interval that can elapse after a gate notification
        /// for a source element to be taken.
        /// </summary>
        [XmlIgnore]
        [Description("The maximum interval that can elapse after a gate notification for a source element to be taken.")]
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the time span for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeSpan))]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Takes the next element from the sequence if this element is produced within
        /// a specified interval after the gate sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="other">
        /// The sequence of gate events. Every time a new gate event is received, the
        /// next element from <paramref name="source"/> is taken if it is produced
        /// before the maximum <see cref="TimeSpan"/> elapses.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Gate(other, TimeSpan);
        }
    }
}
