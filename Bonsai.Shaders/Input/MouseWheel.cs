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
    /// the mouse wheel is moved over the shader window.
    /// </summary>
    [Description("Generates a sequence of events produced whenever the mouse wheel is moved over the shader window.")]
    public class MouseWheel : Source<EventPattern<INativeWindow, MouseWheelEventArgs>>
    {
        /// <summary>
        /// Generates an observable sequence that produces a value whenever the
        /// mouse wheel is moved over the shader window.
        /// </summary>
        /// <returns>
        /// A sequence of events produced whenever the mouse wheel is moved over
        /// the shader window.
        /// </returns>
        public override IObservable<EventPattern<INativeWindow, MouseWheelEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<MouseWheelEventArgs>(
                handler => window.MouseWheel += handler,
                handler => window.MouseWheel -= handler));
        }
    }
}
