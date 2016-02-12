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
    public class Mouse : Source<MouseState>
    {
        public int? Index { get; set; }

        static MouseState GetMouseState(int? index)
        {
            if (index.HasValue) return OpenTK.Input.Mouse.GetState(index.Value);
            else return OpenTK.Input.Mouse.GetState();
        }

        public override IObservable<MouseState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<FrameEventArgs>(
                handler => window.UpdateFrame += handler,
                handler => window.UpdateFrame -= handler)
                .Select(evt => GetMouseState(Index))
                .TakeUntil(window.WindowClosed()));
        }

        public IObservable<MouseState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => GetMouseState(Index));
        }
    }
}
