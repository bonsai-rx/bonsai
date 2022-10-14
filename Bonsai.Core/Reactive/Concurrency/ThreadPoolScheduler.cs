using System.ComponentModel;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the CLR thread pool.
    /// </summary>
    [Description("Returns an object that schedules units of work on the CLR thread pool.")]
    public sealed class ThreadPoolScheduler : SchedulerSource<Rx.ThreadPoolScheduler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolScheduler"/> class.
        /// </summary>
        public ThreadPoolScheduler()
            : base(Rx.ThreadPoolScheduler.Instance)
        {
        }
    }
}
