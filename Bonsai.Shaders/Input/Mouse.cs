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
    [Description("Retrieves the state of the specified mouse device. The position and wheel values are defined in a hardware-specific coordinate system.")]
    public class Mouse : Source<MouseState>
    {
        [Description("The optional index of the mouse device. If it is not specified, the combined state of all devices is retrieved.")]
        public int? Index { get; set; }

        static MouseState GetMouseState(int? index)
        {
            if (index.HasValue) return OpenTK.Input.Mouse.GetState(index.Value);
            else return OpenTK.Input.Mouse.GetState();
        }

        public override IObservable<MouseState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => GetMouseState(Index)));
        }

        public IObservable<MouseState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => GetMouseState(Index));
        }
    }
}
