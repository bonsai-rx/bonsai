using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    [Description("Immediately starts processing the specified shader work queue.")]
    public class DispatchShaderQueue : Sink
    {
        [TypeConverter(typeof(ShaderNameConverter))]
        [Description("The name of the shader program.")]
        public string ShaderName { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.ReserveShader(ShaderName),
                (input, shader) =>
                {
                    shader.Dispatch();
                    return input;
                });
        }
    }
}
