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
    public class MouseLeave : Source<EventPattern<EventArgs>>
    {
        public override IObservable<EventPattern<EventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<EventArgs>(
                handler => window.MouseLeave += handler,
                handler => window.MouseLeave -= handler)
                .TakeUntil(window.WindowClosed()));
        }
    }
}
