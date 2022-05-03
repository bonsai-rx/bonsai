using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that launches one or more compute shader work
    /// groups for each notification in the sequence.
    /// </summary>
    [Description("Launches one or more compute shader work groups for each notification in the sequence.")]
    public class DispatchCompute : Sink
    {
        /// <summary>
        /// Gets or sets the name of the compute shader program.
        /// </summary>
        [TypeConverter(typeof(ComputeProgramNameConverter))]
        [Description("The name of the compute shader program.")]
        public string ShaderName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the number of workgroups to be
        /// launched when dispatching the compute shader.
        /// </summary>
        [Description("Specifies the number of workgroups to be launched when dispatching the compute shader.")]
        public DispatchParameters WorkGroups { get; set; }

        /// <summary>
        /// Launches one or more compute shader work groups whenever the source
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to launch the compute
        /// shader work groups.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of launching one
        /// or more compute shader workgroups whenever the sequence emits a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.ReserveComputeProgram(ShaderName),
                (input, shader) =>
                {
                    shader.Update(() =>
                    {
                        var workGroups = WorkGroups;
                        GL.DispatchCompute(workGroups.NumGroupsX, workGroups.NumGroupsY, workGroups.NumGroupsZ);
                    });
                    return input;
                });
        }
    }
}
