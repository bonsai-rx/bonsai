using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that takes the next element from the sequence
    /// whenever the specified time interval elapses.
    /// </summary>
    [DefaultProperty(nameof(Interval))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Takes the next element from the sequence whenever the specified time interval elapses.")]
    public class GateInterval : Combinator
    {
        /// <summary>
        /// Gets or sets the time interval after which a new element from the sequence is taken.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval after which a new element from the sequence is taken.")]
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
        /// Takes the next element from an observable sequence whenever the specified
        /// time interval elapses.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <returns>The gated observable sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Gate(Interval, HighResolutionScheduler.Default);
        }
    }
}
