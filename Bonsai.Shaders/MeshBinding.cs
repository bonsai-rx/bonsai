using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class MeshBinding : BufferBinding
    {
        int bindingIndex;
        int vertexBuffer;

        public MeshBinding(int index, Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
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
