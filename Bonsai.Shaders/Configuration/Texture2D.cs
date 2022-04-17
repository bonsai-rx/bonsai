using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for two-dimensional
    /// texture resources.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class Texture2D : TextureConfiguration
    {
        /// <summary>
        /// Gets or sets the width of the texture. If no value is specified, the
        /// texture buffer will not be initialized.
        /// </summary>
        [Category("TextureSize")]
        [Description("The width of the texture. If no value is specified, the texture buffer will not be initialized.")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the texture. If no value is specified, the
        /// texture buffer will not be initialized.
        /// </summary>
        [Category("TextureSize")]
        [Description("The height of the texture. If no value is specified, the texture buffer will not be initialized.")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the internal pixel format of the texture.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the internal pixel format of the texture.")]
        public PixelInternalFormat InternalFormat { get; set; } = PixelInternalFormat.Rgba;

        /// <summary>
        /// Gets or sets a value specifying wrapping parameters for the column
        /// coordinates of the texture sampler.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the column coordinates of the texture sampler.")]
        public TextureWrapMode WrapS { get; set; } = TextureWrapMode.Repeat;

        /// <summary>
        /// Gets or sets a value specifying wrapping parameters for the row
        /// coordinates of the texture sampler.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the row coordinates of the texture sampler.")]
        public TextureWrapMode WrapT { get; set; } = TextureWrapMode.Repeat;

        /// <summary>
        /// Gets or sets a value specifying the texture minification filter.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter { get; set; } = TextureMinFilter.Linear;

        /// <summary>
        /// Gets or sets a value specifying the texture magnification filter.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the texture magnification filter.")]
        public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.Linear;

        internal void ConfigureTexture(Texture texture, int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapT);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagFilter);
            if (width > 0 && height > 0)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, width, height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Creates a new empty two-dimensional texture resource, typically used
        /// for uploading dynamic texture data.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Texture"/> class representing
        /// the 2D texture.
        /// </returns>
        /// <inheritdoc/>
        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var texture = new Texture();
            var width = Width.GetValueOrDefault();
            var height = Height.GetValueOrDefault();
            ConfigureTexture(texture, width, height);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }
    }
}
