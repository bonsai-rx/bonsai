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
    [Description("Render primitives using input array data.")]
    public class DrawArrays : Sink<Mat>
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        public DrawArrays()
        {
            Usage = BufferUsageHint.DynamicDraw;
        }

        [TypeConverter(typeof(MaterialNameConverter))]
        [Description("The name of the material shader program.")]
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

        public IObservable<TVertex[]> Process<TVertex>(IObservable<TVertex[]> source) where TVertex : struct
        {
            return Observable.Defer(() =>
            {
                Mesh mesh = null;
                return source.CombineEither(
                    ShaderManager.ReserveMaterial(ShaderName).Do(material =>
                    {
                        material.Update(() =>
                        {
                            mesh = new Mesh();
                            VertexHelper.BindVertexAttributes(
                                mesh.VertexBuffer,
                                mesh.VertexArray,
                                BlittableValueType<TVertex>.Stride,
                                vertexAttributes);
                        });
                    }),
                    (input, material) =>
                    {
                        material.Update(() =>
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
                    ShaderManager.ReserveMaterial(ShaderName),
                    (input, material) =>
                    {
                        material.Update(() =>
                        {
                            if (mesh == null)
                            {
                                mesh = new Mesh();
                                VertexHelper.BindVertexAttributes(
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
                    }).Finally(() =>
                    {
                        if (mesh != null)
                        {
                            mesh.Dispose();
                        }
                    });
            });
        }
    }
}
