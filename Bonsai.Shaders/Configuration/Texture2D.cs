using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class Texture2D : TextureBase
    {
        public Texture2D()
        {
            Name = "tex";
            TextureSlot = TextureUnit.Texture0;
        }

        public TextureUnit TextureSlot { get; set; }

        public override void Load(Shader shader)
        {
            shader.SetTextureSlot(Name, TextureSlot);
            base.Load(shader);
        }

        public override void Bind(Shader shader)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, GetTexture());
        }

        public override void Unbind(Shader shader)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
