using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public abstract class TextureBase : TextureConfiguration
    {
        int texture;

        public TextureBase()
        {
            WrapS = TextureWrapMode.Repeat;
            WrapT = TextureWrapMode.Repeat;
            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMinFilter.Linear;
        }

        [Category("TextureParameter")]
        public TextureWrapMode WrapS { get; set; }

        [Category("TextureParameter")]
        public TextureWrapMode WrapT { get; set; }

        [Category("TextureParameter")]
        public TextureMinFilter MinFilter { get; set; }

        [Category("TextureParameter")]
        public TextureMinFilter MagFilter { get; set; }

        public override void Load(Shader shader)
        {
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapT);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Unload(Shader shader)
        {
            GL.DeleteTextures(1, ref texture);
        }

        public override int GetTexture()
        {
            return texture;
        }
    }
}
