using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that generates a sequence of events produced
    /// whenever it is time to render a new frame.
    /// </summary>
    [Description("Generates a sequence of events produced whenever it is time to render a new frame.")]
    public class RenderFrame : Source<FrameEvent>
    {
        /// <summary>
        /// Generates an observable sequence that emits a notification whenever
        /// it is time to render a new frame.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="FrameEvent"/> objects produced whenever it
        /// is time to render a new frame.
        /// </returns>
        public override IObservable<FrameEvent> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.RenderFrameAsync);
        }
    }
}
