using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the current thread.
    /// </summary>
    [Description("Returns an object that schedules units of work on the current thread.")]
    public sealed class CurrentThreadScheduler : Source<Rx.CurrentThreadScheduler>
    {
        /// <summary>
        /// Generates an observable sequence that returns the singleton
        /// <see cref="Rx.CurrentThreadScheduler"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing the singleton <see cref="Rx.CurrentThreadScheduler"/>
        /// object.
        /// </returns>
        public override IObservable<Rx.CurrentThreadScheduler> Generate()
        {
            return Observable.Return(Rx.CurrentThreadScheduler.Instance);
        }
    }
}
