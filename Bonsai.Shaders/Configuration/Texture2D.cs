using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class Texture2D : TextureConfiguration
    {
        public Texture2D()
        {
            WrapS = TextureWrapMode.Repeat;
            WrapT = TextureWrapMode.Repeat;
            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMinFilter.Linear;
        }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the column coordinates of the texture sampler.")]
        public TextureWrapMode WrapS { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the row coordinates of the texture sampler.")]
        public TextureWrapMode WrapT { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies the texture magnification filter.")]
        public TextureMinFilter MagFilter { get; set; }

        public override Texture CreateResource()
        {
            var texture = new Texture();
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapT);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }
    }
}
