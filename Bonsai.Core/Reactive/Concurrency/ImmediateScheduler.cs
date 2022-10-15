using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// to run immediately on the current thread.
    /// </summary>
    [Description("Returns an object that schedules units of work to run immediately on the current thread.")]
    public sealed class ImmediateScheduler : Source<Rx.ImmediateScheduler>
    {
        /// <summary>
        /// Generates an observable sequence that returns the singleton
        /// <see cref="Rx.ImmediateScheduler"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing the singleton <see cref="Rx.ImmediateScheduler"/>
        /// object.
        /// </returns>
        public override IObservable<Rx.ImmediateScheduler> Generate()
        {
            return Observable.Return(Rx.ImmediateScheduler.Instance);
        }
    }
}
