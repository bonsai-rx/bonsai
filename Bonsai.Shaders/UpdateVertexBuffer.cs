using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the vertex buffer data used by the specified shader.")]
    public class UpdateVertexBuffer : Sink<Mat>
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MeshConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string MeshName { get; set; }

        [Description("Specifies the kind of primitives to render with the vertex buffer data.")]
        public PrimitiveType DrawMode { get; set; }

        public VertexAttributeMappingCollection VertexAttributes
        {
            get { return vertexAttributes; }
        }

        static int GetVertexAttributeSize(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Byte:
                case VertexAttribPointerType.UnsignedByte:
                    return 1;
                case VertexAttribPointerType.Short:
                case VertexAttribPointerType.HalfFloat:
                case VertexAttribPointerType.UnsignedShort:
                    return 2;
                case VertexAttribPointerType.Int:
                case VertexAttribPointerType.Float:
                case VertexAttribPointerType.Fixed:
                case VertexAttribPointerType.UnsignedInt:
                    return 4;
                case VertexAttribPointerType.Double:
                    return 8;
                case VertexAttribPointerType.Int2101010Rev:
                case VertexAttribPointerType.UnsignedInt2101010Rev:
                default:
                    throw new InvalidOperationException("Unsupported attribute type.");
            }
        }

        static void BindVertexAttributes(int vbo, int vao, int stride, VertexAttributeMappingCollection attributes)
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            var offset = 0;
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(
                    i, attribute.Size,
                    attribute.Type,
                    attribute.Normalized,
                    stride, offset);
                offset += attribute.Size * GetVertexAttributeSize(attribute.Type);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public IObservable<TVertex[]> Process<TVertex>(IObservable<TVertex[]> source) where TVertex : struct
        {
            return Observable.Defer(() =>
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
                            mesh = window.Meshes[name];
                            BindVertexAttributes(
                                mesh.VertexBuffer,
                                mesh.VertexArray,
                                BlittableValueType<TVertex>.Stride,
                                vertexAttributes);
                        });
                    }),
                    (input, window) =>
                    {
                        window.Update(() =>
                        {
                            mesh.DrawMode = DrawMode;
                            mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input);
                        });
                        return input;
                    });
            });
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
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
                        if (input.Depth != Depth.F32)
                        {
                            throw new InvalidOperationException("The type of array elements must be 32-bit floating point.");
                        }

                        window.Update(() =>
                        {
                            if (mesh == null)
                            {
                                mesh = window.Meshes[name];
                                BindVertexAttributes(
                                    mesh.VertexBuffer,
                                    mesh.VertexArray,
                                    input.Cols * BlittableValueType<float>.Stride,
                                    vertexAttributes);
                            }

                            mesh.DrawMode = DrawMode;
                            mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input);
                        });
                        return input;
                    });
            });
        }
    }
}
