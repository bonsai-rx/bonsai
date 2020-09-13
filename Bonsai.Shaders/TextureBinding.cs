using OpenTK.Graphics.OpenGL4;

namespace Bonsai.Shaders
{
    class TextureBinding : BufferBinding
    {
        readonly Texture texture;
        readonly TextureUnit textureUnit;
        readonly TextureTarget textureTarget;

        public TextureBinding(Texture source, TextureUnit textureSlot, TextureTarget target)
        {
            texture = source;
            textureUnit = textureSlot;
            textureTarget = target;
        }

        public override void Bind()
        {
            if (texture != null)
            {
                GL.ActiveTexture(textureUnit);
                GL.BindTexture(textureTarget, texture.Id);
            }
        }

        public override void Unbind()
        {
            if (texture != null)
            {
                GL.ActiveTexture(textureUnit);
                GL.BindTexture(textureTarget, 0);
            }
        }
    }
}
