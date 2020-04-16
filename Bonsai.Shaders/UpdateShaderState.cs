using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    [Obsolete]
    [Description("Updates the render state of the specified shader.")]
    public class UpdateShaderState : Sink
    {
        [TypeConverter(typeof(ShaderNameConverter))]
        [Description("The name of the shader program.")]
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
