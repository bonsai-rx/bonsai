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
    [Description("Produces a sequence of events whenever the mouse cursor leaves the shader window bounds.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseLeave : Source<EventPattern<INativeWindow, EventArgs>>
    {
        public override IObservable<EventPattern<INativeWindow, EventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<EventArgs>(
                handler => window.MouseLeave += handler,
                handler => window.MouseLeave -= handler));
        }
    }
}
