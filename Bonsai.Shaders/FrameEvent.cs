using OpenTK;
using System.Reactive;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an update or render frame event raised by the shader window.
    /// </summary>
    public class FrameEvent : EventPattern<INativeWindow, FrameEventArgs>
    {
        internal FrameEvent(ShaderWindow sender, double elapsedTime, FrameEventArgs e)
            : base(sender, e)
        {
            TimeStep = new TimeStep(elapsedTime, e.Time);
        }

        /// <summary>
        /// Gets the amount of time elapsed since the last update.
        /// </summary>
        public TimeStep TimeStep { get; private set; }
    }
}
