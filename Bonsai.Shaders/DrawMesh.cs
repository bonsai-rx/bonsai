using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that draws the specified mesh geometry.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Draws the specified mesh geometry.")]
    public class DrawMesh
    {
        /// <summary>
        /// Gets or sets the name of the material shader program used in the
        /// drawing operation.
        /// </summary>
        [TypeConverter(typeof(MaterialNameConverter))]
        [Description("The name of the material shader program.")]
        public string ShaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the mesh geometry to draw.
        /// </summary>
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to draw.")]
        public string MeshName { get; set; }

        static Action CreateDrawAction(Shader shader, string meshName)
        {
            if (!string.IsNullOrEmpty(meshName))
            {
                var mesh = shader.Window.ResourceManager.Load<Mesh>(meshName);
                return () => mesh.Draw();
            }

            return null;
        }

        /// <summary>
        /// Draws the specified mesh geometry whenever an observable sequence
        /// emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to start drawing the
        /// specified mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of drawing the
        /// specified mesh geometry whenever the sequence emits a notification.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var drawMesh = default(Action);
                var meshName = default(string);
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName),
                    (input, shader) =>
                    {
                        if (meshName != MeshName)
                        {
                            meshName = MeshName;
                            drawMesh = CreateDrawAction(shader, meshName);
                        }

                        shader.Update(drawMesh);
                        return input;
                    });
            });
        }

        /// <summary>
        /// Draws each of the mesh geometries in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mesh"/> objects representing the geometry
        /// to draw. If <see cref="MeshName"/> is specified, the named mesh
        /// geometry will be used instead.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of drawing each
        /// of the mesh geometries in the sequence.
        /// </returns>
        public IObservable<Mesh> Process(IObservable<Mesh> source)
        {
            return Observable.Defer(() =>
            {
                var drawMesh = default(Action);
                var meshName = default(string);
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName),
                    (input, shader) =>
                    {
                        if (meshName != MeshName)
                        {
                            meshName = MeshName;
                            drawMesh = CreateDrawAction(shader, meshName);
                        }

                        shader.Update(drawMesh ?? (() => input.Draw()));
                        return input;
                    });
            });
        }
    }
}
