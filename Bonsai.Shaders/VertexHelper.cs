using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class VertexHelper
    {
        public static int TexturedQuad(int vbo, int vao, bool flipX, bool flipY)
        {
            var vertices = new float[]
            {
                -1f, -1f, flipX ? 1f : 0f, flipY ? 1f : 0f,
                 1f, -1f, flipX ? 0f : 1f, flipY ? 1f : 0f,
                 1f,  1f, flipX ? 0f : 1f, flipY ? 0f : 1f,
                -1f,  1f, flipX ? 1f : 0f, flipY ? 0f : 1f,
            };

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(vertices.Length * BlittableValueType<float>.Stride),
                          vertices,
                          BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(
                0, 2,
                VertexAttribPointerType.Float,
                false,
                4 * BlittableValueType<float>.Stride,
                0 * BlittableValueType<float>.Stride);
            GL.VertexAttribPointer(
                1, 2,
                VertexAttribPointerType.Float,
                false,
                4 * BlittableValueType<float>.Stride,
                2 * BlittableValueType<float>.Stride);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            return 4;
        }

        public static void BindVertexAttributes(int vbo, int vao, int stride, VertexAttributeMappingCollection attributes)
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

        public static int UpdateVertexBuffer<TVertex>(int vertexBuffer, TVertex[] buffer, BufferUsageHint usage)
            where TVertex : struct
        {
            var bufferSize = buffer.Length * BlittableValueType<TVertex>.Stride;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(bufferSize),
                          buffer, usage);
            return buffer.Length;
        }

        public static int UpdateVertexBuffer(int vertexBuffer, Mat buffer, BufferUsageHint usage)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(buffer.Rows * buffer.Step),
                          buffer.Data, usage);
            return buffer.Rows;
        }

        public static int GetVertexAttributeSize(VertexAttribPointerType type)
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
    }
}
