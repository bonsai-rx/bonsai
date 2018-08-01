using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class ImageTextureBinding : BufferBinding
    {
        int textureId;
        TextureUnit textureUnit;
        TextureAccess bindingAccess;
        SizedInternalFormat bindingFormat;

        public ImageTextureBinding(Texture texture, TextureUnit textureSlot, TextureAccess access, SizedInternalFormat format)
        {
            textureId = texture.Id;
            textureUnit = textureSlot;
            bindingAccess = access;
            bindingFormat = format;
        }

        public override void Bind()
        {
            GL.BindImageTexture(
                (int)(textureUnit - TextureUnit.Texture0),
                textureId,
                0, false, 0,
                bindingAccess,
                bindingFormat);
        }

        public override void Unbind()
        {
            GL.BindImageTexture(
                (int)(textureUnit - TextureUnit.Texture0),
                0,
                0, false, 0,
                bindingAccess,
                bindingFormat);
        }
    }
}
