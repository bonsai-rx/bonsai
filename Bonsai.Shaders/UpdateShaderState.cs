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
    [Description("Updates the render state of the specified shader.")]
    public class UpdateShaderState : Sink
    {
        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("Specifies whether the shader is active.")]
        public bool Enabled { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.ReserveShader(ShaderName),
                (input, shader) =>
                {
                    shader.Enabled = Enabled;
                    return input;
                });
        }
    }
}
