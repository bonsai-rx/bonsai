using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Input
{
    [Description("Retrieves the state of the specified keyboard device.")]
    public class Keyboard : Source<KeyboardState>
    {
        [Description("The optional index of the keyboard device. If it is not specified, the combined state of all devices is retrieved.")]
        public int? Index { get; set; }

        static KeyboardState GetKeyboardState(int? index)
        {
            if (index.HasValue) return OpenTK.Input.Keyboard.GetState(index.Value);
            else return OpenTK.Input.Keyboard.GetState();
        }

        public override IObservable<KeyboardState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => GetKeyboardState(Index)));
        }

        public IObservable<KeyboardState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => GetKeyboardState(Index));
        }
    }
}
