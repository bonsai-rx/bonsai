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
    [Description("Produces a sequence of events whenever a key is released while the shader window has focus.")]
    [Editor("Bonsai.Shaders.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class KeyUp : Source<EventPattern<KeyboardKeyEventArgs>>
    {
        public override IObservable<EventPattern<KeyboardKeyEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<KeyboardKeyEventArgs>(
                handler => window.KeyUp += handler,
                handler => window.KeyUp -= handler)
                .TakeUntil(window.WindowClosed()));
        }
    }
}
