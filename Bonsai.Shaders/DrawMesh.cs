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
    [Description("Draws the specified mesh geometry.")]
    public class DrawMesh : Sink
    {
        [Description("The name of the material shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MaterialConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("The name of the mesh geometry to draw.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MeshConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string MeshName { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Create<TSource>(observer =>
            {
                var name = MeshName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A mesh name must be specified.");
                }

                Mesh mesh = null;
                return source.CombineEither(
                    ShaderManager.ReserveMaterial(ShaderName).Do(material =>
                    {
                        material.Update(() =>
                        {
                            if (!material.Window.Meshes.TryGetValue(name, out mesh))
                            {
                                observer.OnError(new InvalidOperationException(string.Format(
                                    "The mesh \"{0}\" was not found.",
                                    name)));
                                return;
                            }
                        });
                    }),
                    (input, material) =>
                    {
                        material.Update(() =>
                        {
                            mesh.Draw();
                        });
                        return input;
                    }).SubscribeSafe(observer);
            });
        }
    }
}
