using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that takes the single next element from the sequence every
    /// time the specified interval elapses.
    /// </summary>
    [DefaultProperty("Interval")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Takes the single next element from the sequence every time the specified interval elapses.")]
    public class GateInterval : Combinator
    {
        /// <summary>
        /// Gets or sets the time interval after which a new element of the sequence is allowed to propagate.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval after which a new value of the sequence is allowed to propagate.")]
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of the interval.
        /// </summary>
        [Browsable(false)]
        [XmlElement("Interval")]
        public string IntervalXml
        {
            get { return XmlConvert.ToString(Interval); }
            set { Interval = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Takes the single next element from the sequence every time the specified
        /// interval elapses.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <returns>The gated observable sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Gate(Interval, HighResolutionScheduler.Default);
        }
    }
}
