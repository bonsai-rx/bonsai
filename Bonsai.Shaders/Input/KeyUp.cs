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
    [Description("Produces a sequence of events whenever a key is released while the shader window has focus.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class KeyUp : Source<EventPattern<INativeWindow, KeyboardKeyEventArgs>>
    {
        [Description("The optional key to use as a filter.")]
        public Key? Key { get; set; }

        [Description("The optional key modifiers to use as a filter.")]
        public KeyModifiers? Modifiers { get; set; }

        public override IObservable<EventPattern<INativeWindow, KeyboardKeyEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<KeyboardKeyEventArgs>(
                handler => window.KeyUp += handler,
                handler => window.KeyUp -= handler))
                .Where(evt =>
                {
                    var args = evt.EventArgs;
                    var key = Key.GetValueOrDefault(args.Key);
                    var modifiers = Modifiers.GetValueOrDefault(args.Modifiers);
                    return args.Key == key && args.Modifiers == modifiers;
                });
        }
    }
}
