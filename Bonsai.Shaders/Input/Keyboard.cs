using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Input
{
    public class Keyboard : Source<KeyboardState>
    {
        public int? Index { get; set; }

        static KeyboardState GetKeyboardState(int? index)
        {
            if (index.HasValue) return OpenTK.Input.Keyboard.GetState(index.Value);
            else return OpenTK.Input.Keyboard.GetState();
        }

        public override IObservable<KeyboardState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<FrameEventArgs>(
                handler => window.UpdateFrame += handler,
                handler => window.UpdateFrame -= handler)
                .Select(evt => GetKeyboardState(Index))
                .TakeUntil(window.WindowClosed()));
        }

        public IObservable<KeyboardState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => GetKeyboardState(Index));
        }
    }
}
