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
    [Description("Render primitives using input array data.")]
    public class DrawArrays : Sink<Mat>
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        public DrawArrays()
        {
            Usage = BufferUsageHint.DynamicDraw;
        }

        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("Specifies the kind of primitives to render with the vertex array data.")]
        public PrimitiveType DrawMode { get; set; }

        [Description("Specifies the expected usage pattern of the vertex buffer.")]
        public BufferUsageHint Usage { get; set; }

        [Description("Specifies the attributes used to interpret the vertex array data.")]
        public VertexAttributeMappingCollection VertexAttributes
        {
            get { return vertexAttributes; }
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
                offset += attribute.Size * VertexHelper.GetVertexAttributeSize(attribute.Type);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public IObservable<TVertex[]> Process<TVertex>(IObservable<TVertex[]> source) where TVertex : struct
        {
            return Observable.Defer(() =>
            {
                Mesh mesh = null;
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName).Do(shader =>
                    {
                        shader.Update(() =>
                        {
                            mesh = new Mesh();
                            BindVertexAttributes(
                                mesh.VertexBuffer,
                                mesh.VertexArray,
                                BlittableValueType<TVertex>.Stride,
                                vertexAttributes);
                        });
                    }),
                    (input, shader) =>
                    {
                        shader.Update(() =>
                        {
                            mesh.DrawMode = DrawMode;
                            mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input, Usage);
                            mesh.Draw();
                        });
                        return input;
                    });
            });
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                Mesh mesh = null;
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName),
                    (input, shader) =>
                    {
                        shader.Update(() =>
                        {
                            if (mesh == null)
                            {
                                mesh = new Mesh();
                                BindVertexAttributes(
                                    mesh.VertexBuffer,
                                    mesh.VertexArray,
                                    input.Cols * input.ElementSize,
                                    vertexAttributes);
                            }

                            mesh.DrawMode = DrawMode;
                            mesh.VertexCount = VertexHelper.UpdateVertexBuffer(mesh.VertexBuffer, input, Usage);
                            mesh.Draw();
                        });
                        return input;
                    });
            });
        }
    }
}
