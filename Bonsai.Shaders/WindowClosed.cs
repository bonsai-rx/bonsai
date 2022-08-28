using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that generates a sequence of events produced
    /// whenever the shader window is closed.
    /// </summary>
    [Description("Generates a sequence of events produced whenever the shader window is closed.")]
    public class WindowClosed : Source<EventPattern<INativeWindow, EventArgs>>
    {
        /// <summary>
        /// Generates an observable sequence that emits a notification whenever
        /// the shader window is closed.
        /// </summary>
        /// <returns>
        /// A sequence of event objects produced whenever the shader window is closed.
        /// </returns>
        public override IObservable<EventPattern<INativeWindow, EventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<EventArgs>(
                handler => window.Closed += handler,
                handler => window.Closed -= handler)
                .Take(1));
        }
    }
}
