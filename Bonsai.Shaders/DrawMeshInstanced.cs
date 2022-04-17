using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that draws the specified mesh geometry using instanced
    /// rendering, where each array in the sequence stores the per-instance attributes.
    /// </summary>
    [Description("Draws the specified mesh geometry using instanced rendering, where each array in the sequence stores the per-instance attributes.")]
    public class DrawMeshInstanced : Sink<Mat>
    {
        readonly InstanceAttributeMappingCollection instanceAttributes = new InstanceAttributeMappingCollection();

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

        /// <summary>
        /// Gets or sets a value specifying the expected usage pattern of the
        /// instance buffer data.
        /// </summary>
        [Description("Specifies the expected usage pattern of the instance buffer data.")]
        public BufferUsageHint Usage { get; set; } = BufferUsageHint.DynamicDraw;

        /// <summary>
        /// Gets a collection of instance attributes specifying how to map instance
        /// buffer data into per-instance input values in the vertex shader.
        /// </summary>
        [Description("Specifies how to map instance buffer data into per-instance input values in the vertex shader.")]
        public InstanceAttributeMappingCollection InstanceAttributes
        {
            get { return instanceAttributes; }
        }

        static void BindInstanceAttributes(MeshInstanced instance, int stride, InstanceAttributeMappingCollection attributes)
        {
            var vertexStride = 0;
            var mesh = instance.InstanceMesh;
            var vertexAttributes = new List<VertexAttributeMapping>();
            GL.BindVertexArray(mesh.VertexArray);
            for (int i = 0; ; i++)
            {
                GL.GetVertexAttrib(i, VertexAttribParameter.ArrayEnabled, out int enabled);
                if (enabled == 0) break;

                var attribute = new VertexAttributeMapping();
                GL.GetVertexAttrib(i, VertexAttribParameter.ArraySize, out int size);
                GL.GetVertexAttrib(i, VertexAttribParameter.ArrayType, out int type);
                GL.GetVertexAttrib(i, VertexAttribParameter.ArrayNormalized, out int normalized);
                GL.GetVertexAttrib(i, VertexAttribParameter.ArrayStride, out int vstride);
                if (vertexStride == 0) vertexStride = vstride;
                else if (vstride != vertexStride)
                {
                    throw new InvalidOperationException("Meshes with interleaved vertex buffers are not supported.");
                }

                attribute.Size = size;
                attribute.Type = (VertexAttribPointerType)type;
                attribute.Normalized = normalized != 0;
                vertexAttributes.Add(attribute);
            }

            var offset = 0;
            GL.BindVertexArray(instance.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VertexBuffer);
            for (int i = 0; i < vertexAttributes.Count; i++)
            {
                var attribute = vertexAttributes[i];
                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(
                    i, attribute.Size,
                    attribute.Type,
                    attribute.Normalized,
                    vertexStride, offset);
                offset += attribute.Size * VertexHelper.GetVertexAttributeSize(attribute.Type);
            }

            offset = 0;
            GL.BindBuffer(BufferTarget.ArrayBuffer, instance.VertexBuffer);
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var index = i + vertexAttributes.Count;
                GL.EnableVertexAttribArray(index);
                GL.VertexAttribPointer(
                    index, attribute.Size,
                    attribute.Type,
                    attribute.Normalized,
                    stride, offset);
                GL.VertexAttribDivisor(index, attribute.Divisor);
                offset += attribute.Size * VertexHelper.GetVertexAttributeSize(attribute.Type);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        MeshInstanced CreateMesh(Shader shader, string meshName, int stride)
        {
            if (string.IsNullOrEmpty(meshName))
            {
                return null;
            }

            var mesh = shader.Window.ResourceManager.Load<Mesh>(meshName);
            var instance = new MeshInstanced(mesh);
            BindInstanceAttributes(instance, stride, instanceAttributes);
            return instance;
        }

        /// <summary>
        /// Draws the specified mesh geometry using instanced rendering, where
        /// each array in an observable sequence stores the per-instance attributes.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the array elements used to store the per-instance attributes.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of per-instance attribute data used to render each instance.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an instanced rendering operation where instance attribute data is drawn
        /// from each of the arrays in the sequence.
        /// </returns>
        public IObservable<TVertex[]> Process<TVertex>(IObservable<TVertex[]> source) where TVertex : struct
        {
            return Observable.Create<TVertex[]>(observer =>
            {
                string meshName = null;
                MeshInstanced instance = null;
                return source.CombineEither(
                    ShaderManager.ReserveMaterial(ShaderName),
                    (input, material) =>
                    {
                        material.Update(() =>
                        {
                            if (meshName != MeshName)
                            {
                                try
                                {
                                    meshName = MeshName;
                                    instance = CreateMesh(material, meshName, BlittableValueType<TVertex>.Stride);
                                }
                                catch (Exception ex)
                                {
                                    observer.OnError(ex);
                                    return;
                                }
                            }

                            if (instance == null) return;
                            if (input != null)
                            {
                                instance.InstanceCount = VertexHelper.UpdateVertexBuffer(instance.VertexBuffer, input, Usage);
                            }
                            instance.Draw();
                        });
                        return input;
                    }).Finally(() =>
                    {
                        if (instance != null)
                        {
                            instance.Dispose();
                        }
                    }).SubscribeSafe(observer);
            });
        }

        /// <summary>
        /// Draws the specified mesh geometry using instanced rendering, where
        /// each of the matrix data in an observable sequence stores the
        /// per-instance attributes.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the per-instance
        /// attribute data used to render each instance.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// an instanced rendering operation where instance attribute data is drawn
        /// from each of the matrices in the sequence.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                string meshName = null;
                MeshInstanced instance = null;
                return source.CombineEither(
                    ShaderManager.ReserveMaterial(ShaderName),
                    (input, material) =>
                    {
                        material.Update(() =>
                        {
                            if (meshName != MeshName || instance == null)
                            {
                                try
                                {
                                    meshName = MeshName;
                                    instance = input != null
                                        ? CreateMesh(material, meshName, stride: input.Cols * input.ElementSize)
                                        : null;
                                }
                                catch (Exception ex)
                                {
                                    observer.OnError(ex);
                                    return;
                                }
                            }

                            if (instance == null) return;
                            if (input != null)
                            {
                                instance.InstanceCount = VertexHelper.UpdateVertexBuffer(instance.VertexBuffer, input, Usage);
                            }
                            instance.Draw();
                        });
                        return input;
                    }).Finally(() =>
                    {
                        if (instance != null)
                        {
                            instance.Dispose();
                        }
                    }).SubscribeSafe(observer);
            });
        }
    }
}
