using System.ComponentModel;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// to run immediately on the current thread.
    /// </summary>
    [Description("Returns an object that schedules units of work to run immediately on the current thread.")]
    public sealed class ImmediateScheduler : SchedulerSource<Rx.ImmediateScheduler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImmediateScheduler"/> class.
        /// </summary>
        public ImmediateScheduler()
            : base(Rx.ImmediateScheduler.Instance)
        {
        }
    }
}
