using System.ComponentModel;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules each unit of work
    /// on a separate thread.
    /// </summary>
    [Description("Returns an object that schedules each unit of work on a separate thread.")]
    public sealed class NewThreadScheduler : SchedulerSource<Rx.NewThreadScheduler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewThreadScheduler"/> class.
        /// </summary>
        public NewThreadScheduler()
            : base(Rx.NewThreadScheduler.Default)
        {
        }
    }
}
