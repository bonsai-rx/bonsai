using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Issues a draw command on the specified shader.")]
    public class DrawShaderAggregate : Sink
    {
        readonly Collection<MeshName> meshNames = new Collection<MeshName>();

        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("The name of the mesh geometry to draw.")]
        public Collection<MeshName> MeshNames
        {
            get { return meshNames; }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                if (meshNames.Count == 0)
                {
                    throw new InvalidOperationException("A mesh name must be specified.");
                }

                MeshAggregate mesh = null;
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName).Do(shader =>
                    {
                        shader.Update(() =>
                        {
                            var meshes = meshNames.Select(meshName => shader.Window.Meshes[meshName.Name]);
                            mesh = new MeshAggregate(meshes);
                        });
                    }),
                    (input, shader) =>
                    {
                        shader.Update(() =>
                        {
                            mesh.Draw();
                        });
                        return input;
                    });
            });
        }
    }
}
