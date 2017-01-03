using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlInclude(typeof(ImageTextureBindingConfiguration))]
    public class TextureBindingConfiguration
    {
        public TextureBindingConfiguration()
        {
            TextureSlot = TextureUnit.Texture0;
        }

        [Description("The name of the uniform sampler binding.")]
        public string Name { get; set; }

        [Description("The slot on which to bind the texture.")]
        public TextureUnit TextureSlot { get; set; }

        [Category("Reference")]
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture that will be bound to the shader.")]
        public string TextureName { get; set; }

        public virtual void Bind(Texture texture)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
        }

        public virtual void Unbind(Texture texture)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
