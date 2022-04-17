using OpenTK.Graphics.OpenGL4;

namespace Bonsai.Shaders
{
    class ImageTextureBinding : BufferBinding
    {
        readonly Texture texture;
        readonly TextureUnit textureUnit;
        readonly TextureAccess bindingAccess;
        readonly SizedInternalFormat bindingFormat;

        public ImageTextureBinding(Texture source, TextureUnit textureSlot, TextureAccess access, SizedInternalFormat format)
        {
            texture = source;
            textureUnit = textureSlot;
            bindingAccess = access;
            bindingFormat = format;
        }

        public override void Bind()
        {
            GL.BindImageTexture(
                textureUnit - TextureUnit.Texture0,
                texture.Id,
                0, false, 0,
                bindingAccess,
                bindingFormat);
        }

        public override void Unbind()
        {
            GL.BindImageTexture(
                textureUnit - TextureUnit.Texture0,
                0,
                0, false, 0,
                bindingAccess,
                bindingFormat);
        }
    }
}
