using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the platform's default scheduler.
    /// </summary>
    [Description("Returns an object that schedules units of work on the platform's default scheduler.")]
    public sealed class DefaultScheduler : Source<Rx.DefaultScheduler>
    {
        /// <summary>
        /// Generates an observable sequence that returns the singleton
        /// <see cref="Rx.DefaultScheduler"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing the singleton <see cref="Rx.DefaultScheduler"/>
        /// object.
        /// </returns>
        public override IObservable<Rx.DefaultScheduler> Generate()
        {
            return Observable.Return(Rx.DefaultScheduler.Instance);
        }
    }
}
