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
    [Description("Retrieves the state of the mouse cursor. The position is defined in absolute desktop points, with the origin placed at the top-left corner of the display.")]
    public class MouseCursor : Source<MouseState>
    {
        public override IObservable<MouseState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<FrameEventArgs>(
                handler => window.UpdateFrame += handler,
                handler => window.UpdateFrame -= handler)
                .Select(evt => OpenTK.Input.Mouse.GetCursorState())
                .TakeUntil(window.WindowClosed()));
        }

        public IObservable<MouseState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => OpenTK.Input.Mouse.GetCursorState());
        }
    }
}
