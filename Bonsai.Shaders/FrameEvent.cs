using OpenTK;
using System.Reactive;

namespace Bonsai.Shaders
{
    public class FrameEvent : EventPattern<INativeWindow, FrameEventArgs>
    {
        internal FrameEvent(ShaderWindow sender, double elapsedTime, FrameEventArgs e)
            : base(sender, e)
        {
            TimeStep = new TimeStep(elapsedTime, e.Time);
        }

        public TimeStep TimeStep { get; private set; }
    }
}
