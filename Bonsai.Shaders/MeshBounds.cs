using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that retrieves the bounds of the specified mesh
    /// geometry.
    /// </summary>
    [Description("Retrieves the bounds of the specified mesh geometry.")]
    public class MeshBounds : Source<Bounds>
    {
        /// <summary>
        /// Gets or sets the name of the mesh geometry for which to retrieve
        /// the bounds.
        /// </summary>
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry for which to retrieve the bounds.")]
        public string MeshName { get; set; }

        /// <summary>
        /// Retrieves the bounds of the specified mesh geometry and surfaces them
        /// through an observable sequence.
        /// </summary>
        /// <returns>
        /// A sequence containing the retrieved mesh <see cref="Bounds"/>.
        /// </returns>
        public override IObservable<Bounds> Generate()
        {
            return Generate(Observable.Return(Unit.Default));
        }

        /// <summary>
        /// Retrieves the bounds of the specified mesh geometry whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for retrieving mesh
        /// bounds.
        /// </param>
        /// <returns>
        /// The sequence of retrieved mesh <see cref="Bounds"/>.
        /// </returns>
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
