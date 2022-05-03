using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that renders primitives using each of the array
    /// data in the sequence.
    /// </summary>
    [Description("Renders primitives using each of the array data in the sequence.")]
    public class DrawArrays : Sink<Mat>
    {
        readonly VertexAttributeMappingCollection vertexAttributes = new VertexAttributeMappingCollection();

        /// <summary>
        /// Gets or sets the name of the material shader program.
        /// </summary>
        [TypeConverter(typeof(MaterialNameConverter))]
        [Description("The name of the material shader program.")]
        public string ShaderName { get; set; }

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
        /// Gets the collection of vertex attributes specifying how to interpret
        /// the vertex array data.
        /// </summary>
        [Description("Specifies the attributes used to interpret the vertex array data.")]
        public VertexAttributeMappingCollection VertexAttributes
        {
            get { return vertexAttributes; }
        }

        /// <summary>
        /// Renders primitives using each of the array data in an observable
        /// sequence.
        /// </summary>
        /// <typeparam name="TVertex">
        /// The type of the array elements used to render each primitive.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of vertex array data used to render each primitive.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// a primitive render operation where vertex data is drawn from each
        /// of the arrays in the sequence.
        /// </returns>
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

        /// <summary>
        /// Renders primitives using each of the matrix data in an observable
        /// sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the vertex array
        /// data used to render each primitive.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of scheduling
        /// a primitive render operation where vertex data is drawn from each
        /// of the matrices in the sequence.
        /// </returns>
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
