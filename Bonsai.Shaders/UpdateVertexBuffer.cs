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
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class UpdateVertexBuffer : Sink<Mat>
    {
        [Editor("Bonsai.Shaders.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        public float LineWidth { get; set; }

        public float PointSize { get; set; }

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

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                var channelCount = 0;
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName).Do(shader =>
                    {
                        shader.Update(() =>
                        {
                            int attribCount;
                            GL.BindVertexArray(shader.VertexBuffer);
                            GL.BindBuffer(BufferTarget.ArrayBuffer, shader.VertexArray);
                            GL.GetProgramInterface(
                                shader.Program,
                                ProgramInterface.ProgramInput,
                                ProgramInterfaceParameter.ActiveResources,
                                out attribCount);

                            for (int index = 0; index <  attribCount; index++)
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

                                GL.EnableVertexAttribArray(index);
                                GL.VertexAttribPointer(
                                    index, channels,
                                    VertexAttribPointerType.Float,
                                    false,
                                    channels * BlittableValueType<float>.Stride,
                                    channelCount);
                                channelCount += channels;
                            }
                            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                            GL.BindVertexArray(0);
                        });
                    }),
                    (input, shader) =>
                    {
                        if (input.Rows > 1 && channelCount != input.Rows)
                        {
                            throw new InvalidOperationException(string.Format(
                                "The size of the input buffer does not match vertex array specification in shader program \"{1}\". Expected {0}-channel buffer, or packed one-dimensional buffer.",
                                channelCount,
                                ShaderName));
                        }

                        var packedBuffer = input;
                        if (channelCount > 1 || input.Rows > 1 && input.Depth != Depth.F32)
                        {
                            packedBuffer = new Mat(1, input.Rows > 1 ? channelCount * input.Cols : input.Cols, Depth.F32, 1);
                            var packedRows = packedBuffer.Reshape(1, packedBuffer.Cols);
                            for (int i = 0; i < input.Rows; i++)
                            {
                                using (var row = input.GetRow(i).Reshape(1, input.Cols))
                                using (var rowStep = packedRows.GetRows(i, packedRows.Rows, input.Rows))
                                {
                                    CV.Convert(row, rowStep);
                                }
                            }
                        }

                        shader.Update(() =>
                        {
                            GL.BindBuffer(BufferTarget.ArrayBuffer, shader.VertexBuffer);
                            GL.BufferData(BufferTarget.ArrayBuffer,
                                          new IntPtr(packedBuffer.Cols * BlittableValueType<float>.Stride),
                                          packedBuffer.Data,
                                          BufferUsageHint.StaticDraw);

                            shader.LineWidth = LineWidth;
                            shader.PointSize = PointSize;
                            shader.VertexCount = input.Cols;
                            shader.DrawMode = DrawMode;
                        });
                        return input;
                    });
            });
        }
    }
}
