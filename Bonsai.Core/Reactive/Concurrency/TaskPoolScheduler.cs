using System.ComponentModel;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the Task Parallel Library (TPL) task pool.
    /// </summary>
    [Description("Returns an object that schedules units of work on the Task Parallel Library (TPL) task pool.")]
    public sealed class TaskPoolScheduler : SchedulerSource<Rx.TaskPoolScheduler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskPoolScheduler"/> class.
        /// </summary>
        public TaskPoolScheduler()
            : base(Rx.TaskPoolScheduler.Default)
        {
        }
    }
}
