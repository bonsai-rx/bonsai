using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the viewport state of the shader window.")]
    public class UpdateViewportState : Sink
    {
        public UpdateViewportState()
        {
            Width = 1;
            Height = 1;
        }

        [Description("The x-coordinate of the lower left corner of the viewport.")]
        public float X { get; set; }

        [Description("The y-coordinate of the lower left corner of the viewport.")]
        public float Y { get; set; }

        [Description("The width of the viewport rectangle.")]
        public float Width { get; set; }

        [Description("The height of the viewport rectangle.")]
        public float Height { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) =>
                {
                    window.Viewport = new RectangleF(X, Y, Width, Height);
                    return input;
                });
        }
    }
}
