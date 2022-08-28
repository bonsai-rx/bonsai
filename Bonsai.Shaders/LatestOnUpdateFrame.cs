using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that replays the latest notification of the
    /// sequence at each update frame event.
    /// </summary>
    [Description("Replays the latest notification of the sequence at each update frame event.")]
    public class LatestOnUpdateFrame : Combinator
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        /// <summary>
        /// Replays the latest notification of an observable sequence at each
        /// update frame event.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The observable sequence whose latest notification will be replayed
        /// at each update frame event.
        /// </param>
        /// <returns>
        /// The sequence of replayed values from the <paramref name="source"/>
        /// sequence, sampled at each update frame event.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var update = updateFrame.Generate();
            return source.CombineLatest(update, (x, evt) => x).SampleSafe(update);
        }
    }
}
