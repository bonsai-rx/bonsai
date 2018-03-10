using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class Mesh : IDisposable
    {
        int vbo;
        int vao;
        int eao;

        public Mesh()
        {
            GL.GenBuffers(1, out vbo);
            GL.GenVertexArrays(1, out vao);
            ElementArrayType = DrawElementsType.UnsignedShort;
        }

        public Bounds Bounds { get; set; }

        public int VertexCount { get; set; }

        public PrimitiveType DrawMode { get; set; }

        public DrawElementsType ElementArrayType { get; set; }

        public int VertexBuffer
        {
            get { return vbo; }
        }

        public int VertexArray
        {
            get { return vao; }
        }

        public int ElementArray
        {
            get { return eao; }
        }

        public void EnsureElementArray()
        {
            if (eao == 0)
            {
                GL.GenBuffers(1, out eao);
            }
        }

        public void Draw()
        {
            var vertexCount = VertexCount;
            if (vertexCount > 0)
            {
                GL.BindVertexArray(vao);
                if (eao > 0)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, eao);
                    GL.DrawElements(DrawMode, vertexCount, ElementArrayType, IntPtr.Zero);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                }
                else GL.DrawArrays(DrawMode, 0, vertexCount);
                GL.BindVertexArray(0);
            }
        }

        public void Dispose()
        {
            if (eao != 0) GL.DeleteBuffers(1, ref eao);
            GL.DeleteVertexArrays(1, ref vao);
            GL.DeleteBuffers(1, ref vbo);
        }
    }
}
