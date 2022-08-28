using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for cubemap texture resources.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class Cubemap : TextureConfiguration
    {
        /// <summary>
        /// Gets or sets the texture size for each of the cubemap faces.
        /// </summary>
        [Category("TextureParameter")]
        [Description("The texture size for each of the cubemap faces.")]
        public int? FaceSize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the internal pixel format of the cubemap.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the internal pixel format of the cubemap.")]
        public PixelInternalFormat InternalFormat { get; set; } = PixelInternalFormat.Rgb;

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

        /// <summary>
        /// Creates a new cubemap texture resource.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Texture"/> class representing
        /// the cubemap texture.
        /// </returns>
        /// <inheritdoc/>
        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var texture = new Texture();
            GL.BindTexture(TextureTarget.TextureCubeMap, texture.Id);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)MagFilter);
            var faceSize = FaceSize.GetValueOrDefault();
            if (faceSize > 0)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
            }
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            return texture;
        }
    }
}
