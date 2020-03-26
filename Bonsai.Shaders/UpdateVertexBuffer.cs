using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the vertex buffer data used by the specified mesh.")]
    public class UpdateVertexBuffer : Sink<Mat>
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        public UpdateVertexBuffer()
        {
            Usage = BufferUsageHint.DynamicDraw;
        }

        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to update.")]
        public string MeshName { get; set; }

        [Description("Specifies the kind of primitives to render with the vertex buffer data.")]
        public PrimitiveType DrawMode { get; set; }

        [Description("Specifies the expected usage pattern of the vertex buffer.")]
        public BufferUsageHint Usage { get; set; }

        [Description("Specifies the attributes used to interpret the vertex buffer data.")]
        public VertexAttributeMappingCollection VertexAttributes
        {
            get { return vertexAttributes; }
        }

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

        public IObservable<Tuple<TVertex[], byte[]>> Process<TVertex>(IObservable<Tuple<TVertex[], byte[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedByte);
        }

        public IObservable<Tuple<TVertex[], short[]>> Process<TVertex>(IObservable<Tuple<TVertex[], short[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

        public IObservable<Tuple<TVertex[], ushort[]>> Process<TVertex>(IObservable<Tuple<TVertex[], ushort[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

        public IObservable<Tuple<TVertex[], int[]>> Process<TVertex>(IObservable<Tuple<TVertex[], int[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

        public IObservable<Tuple<TVertex[], uint[]>> Process<TVertex>(IObservable<Tuple<TVertex[], uint[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

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
