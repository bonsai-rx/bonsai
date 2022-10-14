using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Expressions;
using Bonsai.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that wraps the source sequence in order to run its
    /// subscription and unsubscription logic on the specified scheduler.
    /// </summary>
    /// <remarks>This operator is not commonly used.</remarks>
    /// <seealso cref="ObserveOn"/>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Wraps the source sequence in order to run its subscription and unsubscription logic on the specified scheduler.")]
    public class SubscribeOn : Combinator, ISerializableElement
    {
        /// <summary>
        /// Gets or sets a value specifying the scheduler on which to run subscription and
        /// unsubscription actions.
        /// </summary>
        [XmlElement(Namespace = Constants.XmlNamespace)]
        [TypeConverter(typeof(SchedulerMappingConverter))]
        public SchedulerMapping Scheduler { get; set; } = SchedulerMapping.Default;

        object ISerializableElement.Element => Scheduler;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Scheduler"/> property should be serialized.
        /// </summary>
        [Browsable(false)]
        public bool SchedulerSpecified
        {
            get
            {
                var scheduler = Scheduler;
                return scheduler != SchedulerMapping.Default &&
                    scheduler?.GetType() != typeof(SchedulerMapping);
            }
        }

        /// <summary>
        /// Wraps the source sequence in order to run its subscription and
        /// unsubscription logic on the specified scheduler.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to wrap.</param>
        /// <returns>
        /// An observable sequence where subscription and unsubscription logic
        /// run on the specified scheduler.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.SubscribeOn(Scheduler?.Instance);
        }
    }
}
