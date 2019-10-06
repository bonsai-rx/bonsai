using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class MeshInstanced : IDisposable
    {
        int vbo;
        int vao;
        Mesh instanceMesh;

        internal MeshInstanced(Mesh mesh)
        {
            instanceMesh = mesh;
            GL.GenBuffers(1, out vbo);
            GL.GenVertexArrays(1, out vao);
        }

        public int InstanceCount { get; set; }

        public Mesh InstanceMesh
        {
            get { return instanceMesh; }
        }

        public int VertexBuffer
        {
            get { return vbo; }
        }

        public int VertexArray
        {
            get { return vao; }
        }

        public void Draw()
        {
            var eao = instanceMesh.ElementArray;
            var vertexCount = instanceMesh.VertexCount;
            var instanceCount = InstanceCount;
            if (vertexCount > 0 && instanceCount > 0)
            {
                GL.BindVertexArray(vao);
                if (eao > 0)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, eao);
                    GL.DrawElementsInstanced(instanceMesh.DrawMode, vertexCount, instanceMesh.ElementArrayType, IntPtr.Zero, instanceCount);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                }
                else GL.DrawArraysInstanced(instanceMesh.DrawMode, 0, vertexCount, instanceCount);
                GL.BindVertexArray(0);
            }
        }

        public void Dispose()
        {
            GL.DeleteVertexArrays(1, ref vao);
            GL.DeleteBuffers(1, ref vbo);
        }
    }
}
