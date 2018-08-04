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

        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to draw.")]
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
                            try { mesh = material.Window.ResourceManager.Load<Mesh>(name); }
                            catch (Exception ex) { observer.OnError(ex); }
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
