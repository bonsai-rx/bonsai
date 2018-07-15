using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class TextureBinding : BufferBinding
    {
        Texture texture;
        readonly TextureBindingConfiguration binding;

        public TextureBinding(TextureBindingConfiguration bindingConfiguration)
        {
            if (bindingConfiguration == null)
            {
                throw new ArgumentNullException("bindingConfiguration");
            }

            binding = bindingConfiguration;
        }

        public override void Load(Shader shader)
        {
            shader.SetTextureSlot(binding.Name, binding.TextureSlot);
            if (!string.IsNullOrEmpty(binding.TextureName) && !shader.Window.Textures.TryGetValue(binding.TextureName, out texture))
            {
                throw new InvalidOperationException(string.Format(
                    "The texture reference \"{0}\" was not found.",
                    binding.TextureName));
            }
        }

        public override void Bind(Shader shader)
        {
            if (texture != null)
            {
                GL.ActiveTexture(binding.TextureSlot);
                GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            }
        }

        public override void Unbind(Shader shader)
        {
            if (texture != null)
            {
                GL.ActiveTexture(binding.TextureSlot);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public override void Unload(Shader shader)
        {
            texture = null;
        }
    }
}
