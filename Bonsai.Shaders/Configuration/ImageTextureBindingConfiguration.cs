using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for binding a texture to a shader
    /// image uniform.
    /// </summary>
    [XmlType(TypeName = "ImageTextureBinding", Namespace = Constants.XmlNamespace)]
    public class ImageTextureBindingConfiguration : TextureBindingConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the type of access that will be
        /// performed on the image.
        /// </summary>
        [Description("Specifies the type of access that will be performed on the image.")]
        public TextureAccess Access { get; set; } = TextureAccess.ReadOnly;

        /// <summary>
        /// Gets or sets a value specifying the format of the image elements
        /// when the shader reads or writes image data.
        /// </summary>
        [Description("Specifies the format of the image elements when the shader reads or writes image data.")]
        public SizedInternalFormat Format { get; set; } = SizedInternalFormat.Rgba32f;

        internal override BufferBinding CreateBufferBinding(Shader shader, ResourceManager resourceManager)
        {
            shader.SetTextureSlot(Name, TextureSlot);
            var texture = resourceManager.Load<Texture>(TextureName);
            return new ImageTextureBinding(texture, TextureSlot, Access, Format);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString("BindImageTexture", TextureName);
        }
    }
}
