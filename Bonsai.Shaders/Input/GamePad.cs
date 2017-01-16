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
    [Description("Retrieves the state of the specified gamepad device.")]
    public class GamePad : Source<GamePadState>
    {
        [Description("The index of the gamepad device.")]
        public int Index { get; set; }

        public override IObservable<GamePadState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<FrameEventArgs>(
                handler => window.UpdateFrame += handler,
                handler => window.UpdateFrame -= handler)
                .Select(evt => OpenTK.Input.GamePad.GetState(Index))
                .TakeUntil(window.WindowClosed()));
        }

        public IObservable<GamePadState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => OpenTK.Input.GamePad.GetState(Index));
        }
    }
}
