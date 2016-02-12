using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class ShaderWindowExtensions
    {
        public static IObservable<EventPattern<EventArgs>> WindowClosed(this ShaderWindow window)
        {
            return Observable.FromEventPattern<EventArgs>(
                handler => window.Closed += handler,
                handler => window.Closed -= handler);
        }
    }
}
