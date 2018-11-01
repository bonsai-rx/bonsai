using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class TextureBinding : BufferBinding
    {
        int textureId;
        TextureUnit textureUnit;
        TextureTarget textureTarget;

        public TextureBinding(Texture texture, TextureUnit textureSlot, TextureTarget target)
        {
            textureId = texture != null ? texture.Id : 0;
            textureUnit = textureSlot;
            textureTarget = target;
        }

        public override void Bind()
        {
            if (textureId > 0)
            {
                GL.ActiveTexture(textureUnit);
                GL.BindTexture(textureTarget, textureId);
            }
        }

        public override void Unbind()
        {
            if (textureId > 0)
            {
                GL.ActiveTexture(textureUnit);
                GL.BindTexture(textureTarget, 0);
            }
        }
    }
}
