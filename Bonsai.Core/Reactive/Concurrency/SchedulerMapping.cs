using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Xml.Serialization;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a value specifying the scheduler to be used when handling
    /// concurrency in a reactive operator.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    public struct SchedulerMapping : IEquatable<SchedulerMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerMapping"/> class
        /// using the specified scheduler.
        /// </summary>
        /// <param name="scheduler">The scheduler assigned to the mapping.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        public SchedulerMapping(IScheduler scheduler)
        {
            Instance = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        /// <summary>
        /// Gets or sets the scheduler object assigned to the mapping.
        /// </summary>
        [XmlIgnore]
        public IScheduler Instance { get; private set; }

        /// <summary>
        /// Gets or sets an XML representation of the scheduler instance for serialization.
        /// </summary>
        [XmlText]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string InstanceXml
        {
            get
            {
                var instance = Instance;
                if (instance == Rx.DefaultScheduler.Instance) return nameof(DefaultScheduler);
                if (instance == Rx.CurrentThreadScheduler.Instance) return nameof(CurrentThreadScheduler);
                if (instance == Rx.ImmediateScheduler.Instance) return nameof(ImmediateScheduler);
                if (instance == Rx.NewThreadScheduler.Default) return nameof(NewThreadScheduler);
                if (instance == Rx.TaskPoolScheduler.Default) return nameof(TaskPoolScheduler);
                if (instance == Rx.ThreadPoolScheduler.Instance) return nameof(ThreadPoolScheduler);
                return null;
            }
            set
            {
                Instance = value switch
                {
                    nameof(DefaultScheduler) => Rx.DefaultScheduler.Instance,
                    nameof(CurrentThreadScheduler) => Rx.CurrentThreadScheduler.Instance,
                    nameof(ImmediateScheduler) => Rx.ImmediateScheduler.Instance,
                    nameof(NewThreadScheduler) => Rx.NewThreadScheduler.Default,
                    nameof(TaskPoolScheduler) => Rx.TaskPoolScheduler.Default,
                    nameof(ThreadPoolScheduler) => Rx.ThreadPoolScheduler.Instance,
                    _ => null,
                };
            }
        }

        /// <summary>
        /// Returns a value indicating whether this object has the same scheduler
        /// instance as a specified <see cref="SchedulerMapping"/> value.
        /// </summary>
        /// <param name="other">The <see cref="SchedulerMapping"/> value to compare to this object.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> has the same scheduler instance
        /// as this object; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(SchedulerMapping other)
        {
            return Instance == other.Instance;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SchedulerMapping mapping && Instance == mapping.Instance;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return EqualityComparer<IScheduler>.Default.GetHashCode(Instance);
        }
    }
}
