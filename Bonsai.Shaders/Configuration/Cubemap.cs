using Bonsai.Resources;
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
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class Cubemap : TextureConfiguration
    {
        public Cubemap()
        {
            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMagFilter.Linear;
            InternalFormat = PixelInternalFormat.Rgb;
        }

        [Category("TextureParameter")]
        [Description("The optional texture size for each of the cubemap faces.")]
        public int? FaceSize { get; set; }

        [Category("TextureParameter")]
        [Description("The internal pixel format of the cubemap.")]
        public PixelInternalFormat InternalFormat { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies the texture magnification filter.")]
        public TextureMagFilter MagFilter { get; set; }

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
