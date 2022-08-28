using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that updates the vertex buffer data used by the
    /// specified mesh geometry.
    /// </summary>
    [Description("Updates the vertex buffer data used by the specified mesh geometry.")]
    public class UpdateVertexBuffer : Sink<Mat>
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        /// <summary>
        /// Gets or sets the name of the mesh geometry to update.
        /// </summary>
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to update.")]
        public string MeshName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the kind of primitives to render
        /// with the vertex buffer data.
        /// </summary>
        [Description("Specifies the kind of primitives to render with the vertex buffer data.")]
        public PrimitiveType DrawMode { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the expected usage pattern of the
        /// vertex buffer.
        /// </summary>
        [Description("Specifies the expected usage pattern of the vertex buffer.")]
        public BufferUsageHint Usage { get; set; } = BufferUsageHint.DynamicDraw;

        /// <summary>
        /// Gets the collection of vertex attributes specifying how to interpret
        /// the vertex array data.
        /// </summary>
        [Description("Specifies the attributes used to interpret the vertex buffer data.")]
        public VertexAttributeMappingCollection VertexAttributes
        {
            get { return vertexAttributes; }
        }

        /// <summary>
        /// Updates the specified mesh geometry using each of the array data in
        /// an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the vertex elements used to render each primitive.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of vertex array data used to update the mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an update operation where mesh geometry data is updated using each
        /// of the arrays in the sequence.
        /// </returns>
        public IObservable<TVertex[]> Process<TVertex>(IObservable<TVertex[]> source) where TVertex : struct
        {
            return Observable.Create<TVertex[]>(observer =>
            {
                var name = MeshName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A mesh name must be specified.");
                }

                Mesh mesh = null;
                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            try
                            {
                                mesh = window.ResourceManager.Load<Mesh>(name);
                                VertexHelper.BindVertexAttributes(
                                    mesh.VertexBuffer,
                                    mesh.VertexArray,
                                    BlittableValueType<TVertex>.Stride,
                                    vertexAttributes);
                            }
                            catch (Exception ex) { observer.OnError(ex); }
                        });
                    }),
                    (input, window) =>
                    {
                        window.Update(() =>
                        {
                            mesh.DrawMode = DrawMode;
                            mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input, Usage);
                        });
                        return input;
                    }).SubscribeSafe(observer);
            });
        }

        IObservable<Tuple<TVertex[], TIndex[]>> Process<TVertex, TIndex>(IObservable<Tuple<TVertex[], TIndex[]>> source, DrawElementsType elementType)
            where TVertex : struct
            where TIndex : struct
        {
            return Observable.Create<Tuple<TVertex[], TIndex[]>>(observer =>
            {
                var name = MeshName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A mesh name must be specified.");
                }

                Mesh mesh = null;
                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            try
                            {
                                mesh = window.ResourceManager.Load<Mesh>(name);
                                VertexHelper.BindVertexAttributes(
                                    mesh.VertexBuffer,
                                    mesh.VertexArray,
                                    BlittableValueType<TVertex>.Stride,
                                    vertexAttributes);
                            }
                            catch (Exception ex) { observer.OnError(ex); }
                        });
                    }),
                    (input, window) =>
                    {
                        window.Update(() =>
                        {
                            var indices = input.Item2;
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
                        });
                        return input;
                    }).SubscribeSafe(observer);
            });
        }

        /// <summary>
        /// Updates the specified mesh geometry using vertex and 8-bit index data
        /// from an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the vertex elements used to render each primitive.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of pairs of vertex and 8-bit index data used to update the
        /// mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an update operation where mesh geometry data is updated using each
        /// of the pairs of vertex and index data in the sequence.
        /// </returns>
        public IObservable<Tuple<TVertex[], byte[]>> Process<TVertex>(IObservable<Tuple<TVertex[], byte[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedByte);
        }

        /// <summary>
        /// Updates the specified mesh geometry using vertex and signed 16-bit
        /// index data from an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the vertex elements used to render each primitive.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of pairs of vertex and signed 16-bit index data used to update
        /// the mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an update operation where mesh geometry data is updated using each
        /// of the pairs of vertex and index data in the sequence.
        /// </returns>
        public IObservable<Tuple<TVertex[], short[]>> Process<TVertex>(IObservable<Tuple<TVertex[], short[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

        /// <summary>
        /// Updates the specified mesh geometry using vertex and unsigned 16-bit
        /// index data from an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the vertex elements used to render each primitive.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of pairs of vertex and unsigned 16-bit index data used to update
        /// the mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an update operation where mesh geometry data is updated using each
        /// of the pairs of vertex and index data in the sequence.
        /// </returns>
        public IObservable<Tuple<TVertex[], ushort[]>> Process<TVertex>(IObservable<Tuple<TVertex[], ushort[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

        /// <summary>
        /// Updates the specified mesh geometry using vertex and signed 32-bit
        /// index data from an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the vertex elements used to render each primitive.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of pairs of vertex and signed 32-bit index data used to update
        /// the mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an update operation where mesh geometry data is updated using each
        /// of the pairs of vertex and index data in the sequence.
        /// </returns>
        public IObservable<Tuple<TVertex[], int[]>> Process<TVertex>(IObservable<Tuple<TVertex[], int[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

        /// <summary>
        /// Updates the specified mesh geometry using vertex and unsigned 32-bit
        /// index data from an observable sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the vertex elements used to render each primitive.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of pairs of vertex and unsigned 32-bit index data used to update
        /// the mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an update operation where mesh geometry data is updated using each
        /// of the pairs of vertex and index data in the sequence.
        /// </returns>
        public IObservable<Tuple<TVertex[], uint[]>> Process<TVertex>(IObservable<Tuple<TVertex[], uint[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

        /// <summary>
        /// Updates the specified mesh geometry using each of the matrix data in
        /// an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the vertex array
        /// data used to update the mesh geometry.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an update operation where mesh geometry data is updated using each
        /// of the matrices in the sequence.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                var name = MeshName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A mesh name must be specified.");
                }

                Mesh mesh = null;
                return source.CombineEither(
                    ShaderManager.WindowSource,
                    (input, window) =>
                    {
                        window.Update(() =>
                        {
                            if (mesh == null)
                            {
                                try
                                {
                                    mesh = window.ResourceManager.Load<Mesh>(name);
                                    VertexHelper.BindVertexAttributes(
                                        mesh.VertexBuffer,
                                        mesh.VertexArray,
                                        input.Cols * input.ElementSize,
                                        vertexAttributes);
                                }
                                catch (Exception ex)
                                {
                                    observer.OnError(ex);
                                    return;
                                }
                            }

                            mesh.DrawMode = DrawMode;
                            mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input, Usage);
                        });
                        return input;
                    }).SubscribeSafe(observer);
            });
        }
    }
}
