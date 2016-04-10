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

        public static int UpdateVertexBuffer<TVertex>(int vertexBuffer, TVertex[] buffer)
            where TVertex : struct
        {
            var bufferSize = buffer.Length * BlittableValueType<TVertex>.Stride;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(bufferSize), buffer,
                          BufferUsageHint.StaticDraw);
            return bufferSize;
        }

        public static int UpdateVertexBuffer(int vertexBuffer, Mat buffer)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(buffer.Rows * buffer.Step),
                          buffer.Data,
                          BufferUsageHint.StaticDraw);
            return buffer.Rows;
        }
    }
}
