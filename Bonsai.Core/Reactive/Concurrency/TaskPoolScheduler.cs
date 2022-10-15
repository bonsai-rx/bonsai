using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the Task Parallel Library (TPL) task pool.
    /// </summary>
    [Description("Returns an object that schedules units of work on the Task Parallel Library (TPL) task pool.")]
    public sealed class TaskPoolScheduler : Source<Rx.TaskPoolScheduler>
    {
        /// <summary>
        /// Generates an observable sequence that returns the default
        /// <see cref="Rx.TaskPoolScheduler"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing the default <see cref="Rx.TaskPoolScheduler"/>
        /// object.
        /// </returns>
        public override IObservable<Rx.TaskPoolScheduler> Generate()
        {
            return Observable.Return(Rx.TaskPoolScheduler.Default);
        }
    }
}
