using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the scissor state of the shader window.")]
    public class UpdateScissorState : Sink
    {
        public UpdateScissorState()
        {
            Width = 1;
            Height = 1;
        }

        [Description("The x-coordinate of the lower left corner of the scissor box.")]
        public float X { get; set; }

        [Description("The y-coordinate of the lower left corner of the scissor box.")]
        public float Y { get; set; }

        [Description("The width of the scissor box.")]
        public float Width { get; set; }

        [Description("The height of the scissor box.")]
        public float Height { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) =>
                {
                    window.Scissor = new RectangleF(X, Y, Width, Height);
                    return input;
                });
        }
    }
}
