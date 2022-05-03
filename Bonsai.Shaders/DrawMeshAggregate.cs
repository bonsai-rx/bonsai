using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that aggregates the specified mesh geometry
    /// attributes into a single draw command.
    /// </summary>
    [Description("Aggregates the specified mesh geometries into a single draw command.")]
    public class DrawMeshAggregate : Sink
    {
        readonly Collection<MeshName> meshNames = new Collection<MeshName>();

        /// <summary>
        /// Gets or sets the name of the material shader program used in the
        /// drawing operation.
        /// </summary>
        [TypeConverter(typeof(MaterialNameConverter))]
        [Description("The name of the material shader program.")]
        public string ShaderName { get; set; }

        /// <summary>
        /// Gets the collection of references to pre-declared mesh geometry specifying
        /// the set of attributes to aggregate for drawing.
        /// </summary>
        /// <remarks>
        /// Each mesh geometry will have all its attributes combined into the final
        /// rendered buffer, as if they are all part of the same mesh.
        /// </remarks>
        [Description("Specifies the set of mesh geometry attributes to aggregate for drawing.")]
        public Collection<MeshName> MeshNames
        {
            get { return meshNames; }
        }

        /// <summary>
        /// Draws the aggregate mesh geometry whenever an observable sequence
        /// emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to start drawing the
        /// aggregate mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of drawing the
        /// aggregate mesh geometry whenever the sequence emits a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Create<TSource>(observer =>
            {
                if (meshNames.Count == 0)
                {
                    throw new InvalidOperationException("At least one mesh name must be specified.");
                }

                MeshAggregate mesh = null;
                return source.CombineEither(
                    ShaderManager.ReserveMaterial(ShaderName).Do(material =>
                    {
                        material.Update(() =>
                        {
                            try
                            {
                                var meshAttributes = meshNames.Select(meshName => new MeshAttributeMapping(
                                    material.Window.ResourceManager.Load<Mesh>(meshName.Name),
                                    meshName.Divisor));
                                mesh = new MeshAggregate(meshAttributes);
                            }
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
                    }).Finally(() =>
                    {
                        if (mesh != null)
                        {
                            mesh.Dispose();
                        }
                    }).SubscribeSafe(observer);
            });
        }
    }
}
