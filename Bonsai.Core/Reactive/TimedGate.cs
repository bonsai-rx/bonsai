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
    /// Represents a combinator that takes the single next element from the sequence if this
    /// element is produced within a specified time interval after the gate produces an element.
    /// </summary>
    [DefaultProperty("TimeSpan")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Allows an element of the sequence to propagate if it is produced within a specified interval after the gate sequence produces an element.")]
    public class TimedGate : BinaryCombinator
    {
        /// <summary>
        /// Gets or sets the maximum interval that can elapse after a gate event
        /// for a source element to be propagated.
        /// </summary>
        [XmlIgnore]
        [Description("The maximum interval that can elapse after a gate event for a source element to be propagated.")]
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of the maximum gate time span.
        /// </summary>
        [Browsable(false)]
        [XmlElement("TimeSpan")]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Takes the single next element from the sequence if this element is produced
        /// within a specified time interval after the gate produces an element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TOther">The type of the elements in the sequence of gate events.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="other">
        /// The sequence of gate events. Every time a new gate event is received, the single
        /// next element from <paramref name="source"/> is allowed to propagate if it is
        /// produced before the maximum <see cref="TimeSpan"/> elapses.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Gate(other, TimeSpan);
        }
    }
}
