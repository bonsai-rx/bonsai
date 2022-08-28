using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that immediately starts processing the specified
    /// shader work queue whenever the sequence emits a notification.
    /// </summary>
    [Description("Immediately starts processing the specified shader work queue whenever the sequence emits a notification.")]
    public class DispatchShaderQueue : Sink
    {
        /// <summary>
        /// Gets or sets the name of the shader program.
        /// </summary>
        [TypeConverter(typeof(ShaderNameConverter))]
        [Description("The name of the shader program.")]
        public string ShaderName { get; set; }

        /// <summary>
        /// Immediately starts processing the specified shader work queue
        /// whenever the source sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to start processing
        /// the specified shader work queue.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of immediately
        /// start processing the specified shader work queue whenever the sequence
        /// emits a notification.
        /// </returns>
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
