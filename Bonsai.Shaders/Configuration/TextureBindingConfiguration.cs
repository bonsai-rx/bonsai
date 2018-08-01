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
    [XmlType(TypeName = "TextureBinding")]
    public class TextureBindingConfiguration : BufferBindingConfiguration
    {
        public TextureBindingConfiguration()
        {
            TextureSlot = TextureUnit.Texture0;
        }

        [Description("The slot on which to bind the texture.")]
        public TextureUnit TextureSlot { get; set; }

        [Category("Reference")]
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture that will be bound to the shader.")]
        public string TextureName { get; set; }

        internal override BufferBinding CreateBufferBinding(Shader shader, ResourceManager resourceManager)
        {
            shader.SetTextureSlot(Name, TextureSlot);
            var texture = !string.IsNullOrEmpty(TextureName) ? resourceManager.Load<Texture>(TextureName) : null;
            return new TextureBinding(texture, TextureSlot);
        }

        public override string ToString()
        {
            return ToString("BindTexture", TextureName);
        }
    }
}
