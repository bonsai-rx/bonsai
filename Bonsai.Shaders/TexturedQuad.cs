using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class TexturedQuad : IDisposable
    {
        int vbo;
        int vao;

        public TexturedQuad()
        {
            GL.GenBuffers(1, out vbo);
            GL.GenVertexArrays(1, out vao);
            var vertices = new float[]
            {
                0f, 1f, -1f, -1f,
                1f, 1f,  1f, -1f,
                1f, 0f,  1f,  1f,
                0f, 0f, -1f,  1f,
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
                2 * BlittableValueType<float>.Stride);
            GL.VertexAttribPointer(
                1, 2,
                VertexAttribPointerType.Float,
                false,
                4 * BlittableValueType<float>.Stride, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArrays(1, ref vao);
            GL.DeleteBuffers(1, ref vbo);
        }
    }
}
