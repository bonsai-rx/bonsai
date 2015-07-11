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
    [Description("Produces a sequence of events whenever a key is pressed while the shader window has focus.")]
    [Editor("Bonsai.Shaders.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class KeyDown : Source<KeyboardKeyEventArgs>
    {
        public override IObservable<KeyboardKeyEventArgs> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<KeyboardKeyEventArgs>(
                handler => window.KeyDown += handler,
                handler => window.KeyDown -= handler)
                .Select(evt => evt.EventArgs));
        }
    }
}
