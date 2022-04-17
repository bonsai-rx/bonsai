using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence of events produced
    /// whenever the mouse cursor enters the shader window bounds.
    /// </summary>
    [Description("Generates a sequence of events produced whenever the mouse cursor enters the shader window bounds.")]
    public class MouseEnter : Source<EventPattern<INativeWindow, EventArgs>>
    {
        /// <summary>
        /// Generates an observable sequence that produces a value whenever the
        /// mouse cursor enters the shader window bounds.
        /// </summary>
        /// <returns>
        /// A sequence of events produced whenever the mouse cursor enters the
        /// shader window bounds.
        /// </returns>
        public override IObservable<EventPattern<INativeWindow, EventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<EventArgs>(
                handler => window.MouseEnter += handler,
                handler => window.MouseEnter -= handler));
        }
    }
}
