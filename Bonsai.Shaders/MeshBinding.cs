using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders
{
    class MeshBinding : BufferBinding
    {
        readonly int bindingIndex;
        readonly int vertexBuffer;

        public MeshBinding(int index, Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            bindingIndex = index;
            vertexBuffer = mesh.VertexBuffer;
        }

        public override void Bind()
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingIndex, vertexBuffer);
        }

        public override void Unbind()
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingIndex, 0);
        }
    }
}
