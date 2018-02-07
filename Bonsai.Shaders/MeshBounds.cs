using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Selects the bounds of the specified mesh geometry.")]
    public class MeshBounds : Source<Bounds>
    {
        [Description("The name of the mesh geometry for which to retrieve the bounds.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MeshConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string MeshName { get; set; }

        public override IObservable<Bounds> Generate()
        {
            return Generate(Observable.Return(Unit.Default));
        }

        public IObservable<Bounds> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var name = MeshName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A mesh name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.WindowSource.SelectMany(window =>
                        window.EventPattern<FrameEventArgs>(
                            handler => window.UpdateFrame += handler,
                            handler => window.UpdateFrame -= handler)
                        .Select(evt => window.Meshes[name])),
                    (input, mesh) => mesh.Bounds ?? Bounds.Empty);
            });
        }
    }
}
