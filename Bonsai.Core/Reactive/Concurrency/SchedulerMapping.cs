using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an object that specifies a scheduler to be used when handling
    /// concurrency in a reactive operator.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    public class SchedulerMapping
    {
        internal static readonly SchedulerMapping Default = new DefaultScheduler();

        internal SchedulerMapping()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerMapping"/> class
        /// using the specified scheduler.
        /// </summary>
        /// <param name="scheduler">The scheduler to be assigned to the mapping.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        public SchedulerMapping(IScheduler scheduler)
        {
            Instance = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        internal IScheduler Instance { get; }
    }

    /// <summary>
    /// Represents an operator that returns a scheduler object.
    /// </summary>
    /// <typeparam name="TScheduler">The type of the scheduler object.</typeparam>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    public abstract class SchedulerSource<TScheduler> : SchedulerMapping where TScheduler : IScheduler
    {
        internal SchedulerSource(TScheduler defaultScheduler)
            : base(defaultScheduler)
        {
        }

        /// <summary>
        /// Generates an observable sequence that returns the scheduler instance.
        /// </summary>
        /// <returns>
        /// A sequence containing the <see cref="IScheduler"/> object.
        /// </returns>
        public IObservable<TScheduler> Generate()
        {
            return Observable.Return((TScheduler)Instance);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SchedulerSource<TScheduler> scheduler && scheduler.Instance == Instance;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Instance.GetHashCode();
        }
    }
}
