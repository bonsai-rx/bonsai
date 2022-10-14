using System.ComponentModel;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the platform's default scheduler.
    /// </summary>
    [Description("Returns an object that schedules units of work on the platform's default scheduler.")]
    public sealed class DefaultScheduler : SchedulerSource<Rx.DefaultScheduler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultScheduler"/> class.
        /// </summary>
        public DefaultScheduler()
            : base(Rx.DefaultScheduler.Instance)
        {
        }
    }
}
