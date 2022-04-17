using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that replays the latest notification of the
    /// sequence at each render frame event.
    /// </summary>
    [Description("Replays the latest notification of the sequence at each render frame event.")]
    public class LatestOnRenderFrame : Combinator
    {
        static readonly RenderFrame renderFrame = new RenderFrame();

        /// <summary>
        /// Replays the latest notification of an observable sequence at each
        /// render frame event.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The observable sequence whose latest notification will be replayed
        /// at each render frame event.
        /// </param>
        /// <returns>
        /// The sequence of replayed values from the <paramref name="source"/>
        /// sequence, sampled at each render frame event.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var render = renderFrame.Generate();
            return source.CombineLatest(render, (x, evt) => x).SampleSafe(render);
        }
    }
}
