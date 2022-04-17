using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for binding a texture to a shader
    /// sampler uniform.
    /// </summary>
    [XmlType(TypeName = "TextureBinding", Namespace = Constants.XmlNamespace)]
    public class TextureBindingConfiguration : BufferBindingConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the slot on which to bind the texture.
        /// </summary>
        [Description("Specifies the slot on which to bind the texture.")]
        public TextureUnit TextureSlot { get; set; } = TextureUnit.Texture0;

        /// <summary>
        /// Gets or sets the name of the texture that will be bound to the sampler.
        /// </summary>
        [Category("Reference")]
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture that will be bound to the sampler.")]
        public string TextureName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the texture target that will be bound
        /// to the sampler.
        /// </summary>
        [Category("Reference")]
        [Description("Specifies the texture target that will be bound to the sampler.")]
        public TextureTarget TextureTarget { get; set; } = TextureTarget.Texture2D;

        internal override BufferBinding CreateBufferBinding(Shader shader, ResourceManager resourceManager)
        {
            shader.SetTextureSlot(Name, TextureSlot);
            var texture = !string.IsNullOrEmpty(TextureName) ? resourceManager.Load<Texture>(TextureName) : null;
            return new TextureBinding(texture, TextureSlot, TextureTarget);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString("BindTexture", TextureName);
        }
    }
}
