using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

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
