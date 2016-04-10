using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class TextureBindingConfiguration
    {
        Texture texture;

        public TextureBindingConfiguration()
        {
            TextureSlot = TextureUnit.Texture0;
        }

        public string Name { get; set; }

        public TextureUnit TextureSlot { get; set; }

        [Category("Reference")]
        [TypeConverter(typeof(TextureNameConverter))]
        public string TextureName { get; set; }

        public void Load(Shader shader)
        {
            shader.SetTextureSlot(Name, TextureSlot);
            if (!shader.Window.Textures.TryGetValue(TextureName, out texture))
            {
                throw new InvalidOperationException(string.Format(
                    "The texture reference \"{0}\" was not found.",
                    TextureName));
            }
        }

        public void Bind(Shader shader)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
        }

        public void Unbind(Shader shader)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Unload(Shader shader)
        {
            texture = null;
        }
    }
}
