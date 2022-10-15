using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules units of work
    /// on the CLR thread pool.
    /// </summary>
    [Description("Returns an object that schedules units of work on the CLR thread pool.")]
    public sealed class ThreadPoolScheduler : Source<Rx.ThreadPoolScheduler>
    {
        /// <summary>
        /// Generates an observable sequence that returns the singleton
        /// <see cref="Rx.ThreadPoolScheduler"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing the singleton <see cref="Rx.ThreadPoolScheduler"/>
        /// object.
        /// </returns>
        public override IObservable<Rx.ThreadPoolScheduler> Generate()
        {
            return Observable.Return(Rx.ThreadPoolScheduler.Instance);
        }
    }
}
