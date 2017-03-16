using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Input
{
    [Description("Produces a sequence of events whenever the mouse is moved over the shader window.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseMove : Source<EventPattern<INativeWindow, MouseMoveEventArgs>>
    {
        public override IObservable<EventPattern<INativeWindow, MouseMoveEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<MouseMoveEventArgs>(
                handler => window.MouseMove += handler,
                handler => window.MouseMove -= handler));
        }
    }
}
