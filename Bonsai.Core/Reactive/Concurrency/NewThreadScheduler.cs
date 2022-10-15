using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns an object that schedules each unit of work
    /// on a separate thread.
    /// </summary>
    [Description("Returns an object that schedules each unit of work on a separate thread.")]
    public sealed class NewThreadScheduler : Source<Rx.NewThreadScheduler>
    {
        /// <summary>
        /// Generates an observable sequence that returns the default
        /// <see cref="Rx.NewThreadScheduler"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing the default <see cref="Rx.NewThreadScheduler"/>
        /// object.
        /// </returns>
        public override IObservable<Rx.NewThreadScheduler> Generate()
        {
            return Observable.Return(Rx.NewThreadScheduler.Default);
        }
    }
}
