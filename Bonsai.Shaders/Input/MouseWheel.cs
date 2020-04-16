using OpenTK;
using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    [Description("Produces a sequence of events whenever a mouse wheel is moved over the shader window.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseWheel : Source<EventPattern<INativeWindow, MouseWheelEventArgs>>
    {
        public override IObservable<EventPattern<INativeWindow, MouseWheelEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<MouseWheelEventArgs>(
                handler => window.MouseWheel += handler,
                handler => window.MouseWheel -= handler));
        }
    }
}
