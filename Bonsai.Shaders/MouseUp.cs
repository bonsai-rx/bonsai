using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Produces a sequence of events whenever a mouse button is released over the shader window.")]
    [Editor("Bonsai.Shaders.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseUp : Source<MouseButtonEventArgs>
    {
        public override IObservable<MouseButtonEventArgs> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<MouseButtonEventArgs>(
                handler => window.MouseUp += handler,
                handler => window.MouseUp -= handler)
                .Select(evt => evt.EventArgs));
        }
    }
}
