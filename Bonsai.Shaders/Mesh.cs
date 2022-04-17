using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents a collection of vertices and indices used to describe
    /// arbitrary geometry.
    /// </summary>
    public class Mesh : IDisposable
    {
        int vbo;
        int vao;
        int eao;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh()
        {
            GL.GenBuffers(1, out vbo);
            GL.GenVertexArrays(1, out vao);
            ElementArrayType = DrawElementsType.UnsignedShort;
        }

        /// <summary>
        /// Gets or sets the axis-aligned bounding box of the mesh vertices.
        /// </summary>
        public Bounds Bounds { get; set; }

        /// <summary>
        /// Gets or sets the number of vertices in the mesh vertex buffer.
        /// </summary>
        public int VertexCount { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the draw mode to use for rendering
        /// the mesh.
        /// </summary>
        public PrimitiveType DrawMode { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the type of the elements in the element
        /// array object.
        /// </summary>
        public DrawElementsType ElementArrayType { get; set; }

        /// <summary>
        /// Gets the handle to the vertex buffer object.
        /// </summary>
        public int VertexBuffer
        {
            get { return vbo; }
        }

        /// <summary>
        /// Gets the handle to the vertex array object.
        /// </summary>
        public int VertexArray
        {
            get { return vao; }
        }

        /// <summary>
        /// Gets the handle to the element array object.
        /// </summary>
        public int ElementArray
        {
            get { return eao; }
        }

        /// <summary>
        /// Ensures the element array object is initialized.
        /// </summary>
        public void EnsureElementArray()
        {
            if (eao == 0)
            {
                GL.GenBuffers(1, out eao);
            }
        }

        /// <summary>
        /// Renders primitives from mesh vertex array data.
        /// </summary>
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

        /// <summary>
        /// Releases all resources used by the <see cref="Mesh"/> class.
        /// </summary>
        public void Dispose()
        {
            if (eao != 0) GL.DeleteBuffers(1, ref eao);
            GL.DeleteVertexArrays(1, ref vao);
            GL.DeleteBuffers(1, ref vbo);
        }
    }
}
