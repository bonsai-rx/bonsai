using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that raises an error if the next element is not received
    /// within the specified timeout duration from the previous element.
    /// </summary>
    [DefaultProperty("DueTime")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Raises an error if the next element is not received within the specified timeout duration from the previous element.")]
    public class Timeout : Combinator
    {
        /// <summary>
        /// Gets or sets the maximum duration between values before a timeout occurs.
        /// </summary>
        [XmlIgnore]
        [Description("The maximum duration between values before a timeout occurs.")]
        public TimeSpan DueTime { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of the timeout duration.
        /// </summary>
        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Applies a timeout policy for each element in the observable sequence. If the
        /// next element is not received within the specified timeout duration from the previous
        /// element, a <see cref="TimeoutException"/> is propagated to the observer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to perform a timeout for.</param>
        /// <returns>The sequence with a <see cref="TimeoutException"/> in case of a timeout.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Timeout(DueTime, HighResolutionScheduler.Default);
        }
    }
}
