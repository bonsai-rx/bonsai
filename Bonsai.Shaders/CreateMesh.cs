using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a new mesh geometry for each vertex
    /// array data in the sequence.
    /// </summary>
    [Combinator]
    [Description("Creates a new mesh geometry for each vertex array data in the sequence.")]
    public class CreateMesh
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        /// <summary>
        /// Gets or sets a value specifying the kind of primitives to render
        /// with the vertex array data.
        /// </summary>
        [Description("Specifies the kind of primitives to render with the vertex array data.")]
        public PrimitiveType DrawMode { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the expected usage pattern of the
        /// vertex buffer.
        /// </summary>
        [Description("Specifies the expected usage pattern of the vertex buffer.")]
        public BufferUsageHint Usage { get; set; } = BufferUsageHint.DynamicDraw;

        /// <summary>
        /// Gets a collection of vertex attributes specifying how to map vertex
        /// array data into user-defined input values in the vertex shader.
        /// </summary>
        [Description("Specifies how to map vertex array data into user-defined input values in the vertex shader.")]
        public VertexAttributeMappingCollection VertexAttributes
        {
            get { return vertexAttributes; }
        }

        /// <summary>
        /// Creates a new mesh geometry for each vertex array data in an observable
        /// sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the values used to represent each vertex in the mesh.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of vertex array data representing the geometry used to
        /// create each new mesh.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mesh"/> objects storing all vertex data for
        /// each array in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Mesh> Process<TVertex>(IObservable<TVertex[]> source) where TVertex : struct
        {
            return source.Select(input =>
            {
                var mesh = new Mesh();
                VertexHelper.BindVertexAttributes(
                    mesh.VertexBuffer,
                    mesh.VertexArray,
                    BlittableValueType<TVertex>.Stride,
                    vertexAttributes);
                mesh.DrawMode = DrawMode;
                mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input, Usage);
                return mesh;
            });
        }

        IObservable<Mesh> Process<TVertex, TIndex>(IObservable<Tuple<TVertex[], TIndex[]>> source, DrawElementsType elementType)
            where TVertex : struct
            where TIndex : struct
        {
            return source.Select(input =>
            {
                var mesh = new Mesh();
                var indices = input.Item2;
                VertexHelper.BindVertexAttributes(
                    mesh.VertexBuffer,
                    mesh.VertexArray,
                    BlittableValueType<TVertex>.Stride,
                    vertexAttributes);
                mesh.EnsureElementArray();
                mesh.DrawMode = DrawMode;
                mesh.VertexCount = indices.Length;
                mesh.ElementArrayType = elementType;
                VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input.Item1, Usage);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ElementArray);
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              new IntPtr(indices.Length * BlittableValueType<TIndex>.Stride),
                              indices,
                              Usage);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                return mesh;
            });
        }

        /// <summary>
        /// Creates a new mesh geometry for each pair of vertex and index data
        /// in an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the values used to represent each vertex in the mesh.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs containing the vertex and index data representing
        /// the geometry used to create each new mesh, where each vertex index is
        /// stored as an 8-bit unsigned integer.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mesh"/> objects storing geometry specified by
        /// each pair of vertex and index data in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], byte[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedByte);
        }

        /// <summary>
        /// Creates a new mesh geometry for each pair of vertex and index data
        /// in an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the values used to represent each vertex in the mesh.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs containing the vertex and index data representing
        /// the geometry used to create each new mesh, where each vertex index is
        /// stored as a 32-bit signed integer.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mesh"/> objects storing geometry specified by
        /// each pair of vertex and index data in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], int[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

        /// <summary>
        /// Creates a new mesh geometry for each pair of vertex and index data
        /// in an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the values used to represent each vertex in the mesh.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs containing the vertex and index data representing
        /// the geometry used to create each new mesh, where each vertex index is
        /// stored as a 32-bit unsigned integer.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mesh"/> objects storing geometry specified by
        /// each pair of vertex and index data in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], uint[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

        /// <summary>
        /// Creates a new mesh geometry for each pair of vertex and index data
        /// in an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the values used to represent each vertex in the mesh.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs containing the vertex and index data representing
        /// the geometry used to create each new mesh, where each vertex index is
        /// stored as a 16-bit signed integer.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mesh"/> objects storing geometry specified by
        /// each pair of vertex and index data in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], short[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

        /// <summary>
        /// Creates a new mesh geometry for each pair of vertex and index data
        /// in an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the values used to represent each vertex in the mesh.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs containing the vertex and index data representing
        /// the geometry used to create each new mesh, where each vertex index is
        /// stored as a 16-bit unsigned integer.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mesh"/> objects storing geometry specified by
        /// each pair of vertex and index data in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], ushort[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

        /// <summary>
        /// Creates a new mesh geometry for each vertex array data in an observable
        /// sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of multi-channel matrices storing the geometry data used
        /// to create each new mesh. Each row in the matrix represents the data for
        /// one vertex.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mesh"/> objects storing all vertex data for
        /// each multi-channel matrix in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Mesh> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var mesh = new Mesh();
                VertexHelper.BindVertexAttributes(
                    mesh.VertexBuffer,
                    mesh.VertexArray,
                    input.Cols * input.ElementSize,
                    vertexAttributes);
                mesh.DrawMode = DrawMode;
                mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input, Usage);
                return mesh;
            });
        }
    }
}
