using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class ImageTextureBinding : BufferBinding
    {
        Texture texture;
        readonly ImageTextureBindingConfiguration binding;

        public ImageTextureBinding(ImageTextureBindingConfiguration bindingConfiguration)
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
            if (!shader.Window.Textures.TryGetValue(binding.TextureName, out texture))
            {
                throw new InvalidOperationException(string.Format(
                    "The texture reference \"{0}\" was not found.",
                    binding.TextureName));
            }
        }

        public override void Bind(Shader shader)
        {
            GL.BindImageTexture(
                (int)(binding.TextureSlot - TextureUnit.Texture0),
                texture.Id,
                0, false, 0,
                binding.Access,
                binding.Format);
        }

        public override void Unbind(Shader shader)
        {
            GL.BindImageTexture(
                (int)(binding.TextureSlot - TextureUnit.Texture0),
                0,
                0, false, 0,
                binding.Access,
                binding.Format);
        }

        public override void Unload(Shader shader)
        {
            texture = null;
        }
    }
}
