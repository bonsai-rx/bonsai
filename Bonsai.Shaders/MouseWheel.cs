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
    [Description("Produces a sequence of events whenever a mouse wheel is moved over the shader window.")]
    [Editor("Bonsai.Shaders.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseWheel : Source<MouseWheelEventArgs>
    {
        public override IObservable<MouseWheelEventArgs> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<MouseWheelEventArgs>(
                handler => window.MouseWheel += handler,
                handler => window.MouseWheel -= handler)
                .Select(evt => evt.EventArgs));
        }
    }
}
