using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that issues a shader draw call. This type is
    /// obsolete, please use <see cref="DrawMesh"/> instead.
    /// </summary>
    [Obsolete]
    [Description("Issues a shader draw call. This type is obsolete, please use DrawMesh instead.")]
    public class DrawShader : Sink
    {
        readonly DrawMesh drawMesh = new DrawMesh();

        /// <summary>
        /// Gets or sets the name of the material shader program used in the
        /// drawing operation.
        /// </summary>
        [TypeConverter(typeof(MaterialNameConverter))]
        [Description("The name of the material shader program used in the drawing operation.")]
        public string ShaderName
        {
            get { return drawMesh.ShaderName; }
            set { drawMesh.ShaderName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the mesh geometry to draw.
        /// </summary>
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to draw.")]
        public string MeshName
        {
            get { return drawMesh.MeshName; }
            set { drawMesh.MeshName = value; }
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
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return drawMesh.Process(source);
        }
    }
}
