using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    [Description("Produces a sequence of events whenever the mouse cursor enters the shader window bounds.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseEnter : Source<EventPattern<INativeWindow, EventArgs>>
    {
        public override IObservable<EventPattern<INativeWindow, EventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<EventArgs>(
                handler => window.MouseEnter += handler,
                handler => window.MouseEnter -= handler));
        }
    }
}
