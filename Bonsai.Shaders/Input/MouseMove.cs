using OpenTK;
using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence of events produced whenever
    /// the mouse is moved over the shader window.
    /// </summary>
    [Description("Generates a sequence of events produced whenever the mouse is moved over the shader window.")]
    public class MouseMove : Source<EventPattern<INativeWindow, MouseMoveEventArgs>>
    {
        /// <summary>
        /// Generates an observable sequence that produces a value whenever the
        /// mouse is moved over the shader window.
        /// </summary>
        /// <returns>
        /// A sequence of events produced whenever the mouse is moved over the
        /// shader window.
        /// </returns>
        public override IObservable<EventPattern<INativeWindow, MouseMoveEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<MouseMoveEventArgs>(
                handler => window.MouseMove += handler,
                handler => window.MouseMove -= handler));
        }
    }
}
