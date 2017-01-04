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
        Mesh mesh;
        readonly MeshBindingConfiguration binding;

        public MeshBinding(MeshBindingConfiguration bindingConfiguration)
        {
            if (bindingConfiguration == null)
            {
                throw new ArgumentNullException("bindingConfiguration");
            }

            binding = bindingConfiguration;
        }

        public override void Load(Shader shader)
        {
            if (!shader.Window.Meshes.TryGetValue(binding.MeshName, out mesh))
            {
                throw new InvalidOperationException(string.Format(
                    "The mesh reference \"{0}\" was not found.",
                    binding.MeshName));
            }
        }

        public override void Bind(Shader shader)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding.Index, mesh.VertexBuffer);
        }

        public override void Unbind(Shader shader)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding.Index, 0);
        }

        public override void Unload(Shader shader)
        {
            mesh = null;
        }
    }
}
