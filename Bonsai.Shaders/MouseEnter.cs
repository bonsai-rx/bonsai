using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Produces a sequence of events whenever the mouse cursor enters the shader window bounds.")]
    [Editor("Bonsai.Shaders.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseEnter : Source<EventPattern<EventArgs>>
    {
        public override IObservable<EventPattern<EventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<EventArgs>(
                handler => window.MouseEnter += handler,
                handler => window.MouseEnter -= handler));
        }
    }
}
