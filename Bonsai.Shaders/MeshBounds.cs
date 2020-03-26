using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry for which to retrieve the bounds.")]
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
                    ShaderManager.WindowSource,
                    (input, window) =>
                    {
                        var mesh = window.ResourceManager.Load<Mesh>(name);
                        return mesh.Bounds ?? Bounds.Empty;
                    });
            });
        }
    }
}
