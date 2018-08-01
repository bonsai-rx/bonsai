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
    [Description("Updates the vertex buffer data used by the specified mesh.")]
    public class UpdateVertexBuffer : Sink<Mat>
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        public UpdateVertexBuffer()
        {
            Usage = BufferUsageHint.DynamicDraw;
        }

        [Description("The name of the mesh geometry to update.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MeshConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
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
