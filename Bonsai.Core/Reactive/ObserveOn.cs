using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that sends all notifications in the sequence through
    /// the specified scheduler.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Sends all notifications in the sequence through the specified scheduler.")]
    public class ObserveOn : Combinator
    {
        /// <summary>
        /// Gets or sets a value specifying the scheduler on which to emit notifications.
        /// </summary>
        [TypeConverter(typeof(SchedulerMappingConverter))]
        [Description("Specifies the scheduler on which to emit notifications.")]
        public SchedulerMapping Scheduler { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Scheduler"/> property should be serialized.
        /// </summary>
        [Browsable(false)]
        public bool SchedulerSpecified
        {
            get { return !string.IsNullOrEmpty(Scheduler.InstanceXml); }
        }

        /// <summary>
        /// Sends all notifications in an observable sequence through the specified scheduler.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to schedule notifications for.</param>
        /// <returns>
        /// An observable sequence where all notifications are sent on the specified
        /// scheduler.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.ObserveOn(Scheduler.Instance);
        }
    }
}
