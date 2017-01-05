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
    [Description("Launches one or more compute shader work groups.")]
    public class DispatchCompute : Sink
    {
        [Description("The name of the compute shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.ComputeConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("Specifies the number of workgroups to be launched when dispatching the compute shader.")]
        public DispatchParameters WorkGroups { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.ReserveCompute(ShaderName),
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
