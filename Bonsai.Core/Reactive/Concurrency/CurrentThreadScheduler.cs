using System.ComponentModel;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the current thread.
    /// </summary>
    [Description("Returns an object that schedules units of work on the current thread.")]
    public sealed class CurrentThreadScheduler : SchedulerSource<Rx.CurrentThreadScheduler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentThreadScheduler"/> class.
        /// </summary>
        public CurrentThreadScheduler()
            : base(Rx.CurrentThreadScheduler.Instance)
        {
        }
    }
}
