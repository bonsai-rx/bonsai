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
        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("Specifies the kind of primitives to render with the vertex buffer data.")]
        public PrimitiveType DrawMode { get; set; }

        static int GetAttribPointerChannels(ActiveAttribType type)
        {
            switch (type)
            {
                case ActiveAttribType.Float: return 1;
                case ActiveAttribType.FloatVec2: return 2;
                case ActiveAttribType.FloatVec3: return 3;
                case ActiveAttribType.FloatVec4: return 4;
                default: return 0;
            }
        }

        int GetVertexChannels(Shader shader)
        {
            int attribCount;
            GL.BindVertexArray(shader.VertexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, shader.VertexArray);
            GL.GetProgramInterface(
                shader.Program,
                ProgramInterface.ProgramInput,
                ProgramInterfaceParameter.ActiveResources,
                out attribCount);

            var channelCount = 0;
            var attribChannels = new int[attribCount];
            for (int index = 0; index < attribChannels.Length; index++)
            {
                int attribSize;
                ActiveAttribType attribType;
                var name = GL.GetActiveAttrib(shader.Program, index, out attribSize, out attribType);
                var channels = GetAttribPointerChannels(attribType);
                if (attribSize != 1 || channels < 1)
                {
                    throw new InvalidOperationException(string.Format(
                        "The type of vertex attribute \"{0}\" is not supported in shader program \"{1}\".",
                        name,
                        ShaderName));
                }

                channelCount += channels;
                attribChannels[index] = channels;
            }

            var offset = 0;
            for (int index = 0; index < attribChannels.Length; index++)
            {
                GL.EnableVertexAttribArray(index);
                GL.VertexAttribPointer(
                    index, attribChannels[index],
                    VertexAttribPointerType.Float,
                    false,
                    channelCount * BlittableValueType<float>.Stride,
                    offset);
                offset += attribChannels[index] * BlittableValueType<float>.Stride;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            return channelCount;
        }

        int ProcessBuffer(int vertexBuffer, int channelCount, Mat buffer)
        {
            if (buffer.Rows > 1 && buffer.Cols > 1 && buffer.Cols != channelCount)
            {
                throw new InvalidOperationException(string.Format(
                    "The size of the input buffer does not match vertex array specification in shader program \"{1}\". Expected {0}-channel buffer, or packed one-dimensional buffer.",
                    channelCount,
                    ShaderName));
            }

            return VertexHelper.UpdateVertexBuffer(vertexBuffer, buffer);
        }

        public IObservable<TVertex[]> Process<TVertex>(IObservable<TVertex[]> source) where TVertex : struct
        {
            return Observable.Defer(() =>
            {
                var channelCount = 0;
                var buffer = default(TVertex[]);
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName).Do(shader =>
                    {
                        shader.Update(() =>
                        {
                            channelCount = GetVertexChannels(shader);
                        });
                    }),
                    (input, shader) =>
                    {
                        if (Interlocked.Exchange(ref buffer, input) == null)
                        {
                            shader.Update(() =>
                            {
                                shader.DrawMode = DrawMode;
                                shader.VertexCount = VertexHelper.UpdateVertexBuffer(shader.VertexBuffer, channelCount, buffer);
                                Interlocked.Exchange(ref buffer, null);
                            });
                        }
                        return input;
                    });
            });
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                var channelCount = 0;
                var buffer = default(Mat);
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName).Do(shader =>
                    {
                        shader.Update(() =>
                        {
                            channelCount = GetVertexChannels(shader);
                        });
                    }),
                    (input, shader) =>
                    {
                        if (input.Depth != Depth.F32)
                        {
                            throw new InvalidOperationException("The type of array elements must be 32-bit floating point.");
                        }

                        if (Interlocked.Exchange(ref buffer, input) == null)
                        {
                            shader.Update(() =>
                            {
                                shader.DrawMode = DrawMode;
                                shader.VertexCount = ProcessBuffer(shader.VertexBuffer, channelCount, buffer);
                                Interlocked.Exchange(ref buffer, null);
                            });
                        }
                        return input;
                    });
            });
        }
    }
}
