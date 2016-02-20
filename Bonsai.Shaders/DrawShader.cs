using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Issues a draw command on the specified shader.")]
    public class DrawShader : Sink
    {
        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("Specifies the kind of primitives to render with the shader vertex data.")]
        public PrimitiveType? DrawMode { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.ReserveShader(ShaderName),
                (input, shader) =>
                {
                    shader.Update(() =>
                    {
                        var drawMode = DrawMode;
                        if(drawMode.HasValue) shader.DrawMode = drawMode.Value;
                        shader.Draw();
                    });
                    return input;
                });
        }
    }
}
