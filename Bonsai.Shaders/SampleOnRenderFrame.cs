using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that samples notifications from the sequence
    /// whenever there is a new render frame event.
    /// </summary>
    [Description("Samples notifications from the sequence whenever there is a new render frame event.")]
    public class SampleOnRenderFrame : Combinator
    {
        static readonly RenderFrame renderFrame = new RenderFrame();

        /// <summary>
        /// Samples notifications from an observable sequence whenever there is
        /// a new render frame event.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The observable sequence whose notifications will be sampled
        /// at each render frame event.
        /// </param>
        /// <returns>
        /// The sequence of sampled notifications from the <paramref name="source"/>
        /// sequence, emitted at each render frame event.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var render = renderFrame.Generate();
            return source.SampleSafe(render);
        }
    }
}
