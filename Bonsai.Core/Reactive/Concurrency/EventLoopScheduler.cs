using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that creates an object that schedules units of work
    /// on a single dedicated thread.
    /// </summary>
    [Description("Creates an object that schedules units of work on a single dedicated thread.")]
    public sealed class EventLoopScheduler : Source<Rx.EventLoopScheduler>
    {
        /// <summary>
        /// Generates an observable sequence that returns a new
        /// <see cref="Rx.EventLoopScheduler"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Rx.EventLoopScheduler"/> object.
        /// </returns>
        public override IObservable<Rx.EventLoopScheduler> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Rx.EventLoopScheduler()));
        }
    }
}
