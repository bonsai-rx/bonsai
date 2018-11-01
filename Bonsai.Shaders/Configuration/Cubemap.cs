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
            WrapS = TextureWrapMode.ClampToEdge;
            WrapT = TextureWrapMode.ClampToEdge;
            WrapR = TextureWrapMode.ClampToEdge;
            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMagFilter.Linear;
            InternalFormat = PixelInternalFormat.Rgb;
        }

        [Category("TextureSize")]
        [Description("The optional width of the cubemap.")]
        public int? Width { get; set; }

        [Category("TextureSize")]
        [Description("The optional height of the cubemap.")]
        public int? Height { get; set; }

        [Category("TextureParameter")]
        [Description("The internal pixel format of the cubemap.")]
        public PixelInternalFormat InternalFormat { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the X coordinate of the cubemap sampler.")]
        public TextureWrapMode WrapS { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the Y coordinate of the cubemap sampler.")]
        public TextureWrapMode WrapT { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the Z coordinate of the cubemap sampler.")]
        public TextureWrapMode WrapR { get; set; }

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
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)WrapS);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)WrapT);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)WrapR);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)MagFilter);
            var width = Width.GetValueOrDefault();
            var height = Height.GetValueOrDefault();
            if (width > 0 && height > 0)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, InternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, InternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, InternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, InternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, InternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, InternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            }
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            return texture;
        }
    }
}
