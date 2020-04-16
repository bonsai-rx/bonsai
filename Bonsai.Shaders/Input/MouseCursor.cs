using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    [Description("Retrieves the state of the mouse cursor. The position is defined in absolute desktop points, with the origin placed at the top-left corner of the display.")]
    public class MouseCursor : Source<MouseState>
    {
        public override IObservable<MouseState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => OpenTK.Input.Mouse.GetCursorState()));
        }

        public IObservable<MouseState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => OpenTK.Input.Mouse.GetCursorState());
        }
    }
}
