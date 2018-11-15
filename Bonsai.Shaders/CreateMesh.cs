using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Combinator]
    [Description("Creates a new mesh geometry using input array data.")]
    public class CreateMesh
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        public CreateMesh()
        {
            Usage = BufferUsageHint.DynamicDraw;
        }

        [Description("Specifies the kind of primitives to render with the vertex array data.")]
        public PrimitiveType DrawMode { get; set; }

        [Description("Specifies the expected usage pattern of the vertex buffer.")]
        public BufferUsageHint Usage { get; set; }

        [Description("Specifies the attributes used to interpret the vertex array data.")]
        public VertexAttributeMappingCollection VertexAttributes
        {
            get { return vertexAttributes; }
        }

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

        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], byte[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedByte);
        }

        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], int[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], uint[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedInt);
        }

        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], short[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

        public IObservable<Mesh> Process<TVertex>(IObservable<Tuple<TVertex[], ushort[]>> source) where TVertex : struct
        {
            return Process(source, DrawElementsType.UnsignedShort);
        }

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
