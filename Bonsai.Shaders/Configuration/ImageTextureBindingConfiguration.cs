using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class ImageTextureBindingConfiguration : TextureBindingConfiguration
    {
        [Description("The type of access that will be performed on the image.")]
        public TextureAccess Access { get; set; }

        [Description("The format that the elements of the image will be treated as for the purposes of formatted stores.")]
        public SizedInternalFormat Format { get; set; }

        public override void Bind(Texture texture)
        {
            GL.BindImageTexture(
                (int)(TextureSlot - TextureUnit.Texture0),
                texture.Id,
                0, false, 0,
                Access,
                Format);
        }

        public override void Unbind(Texture texture)
        {
            GL.BindImageTexture(
                (int)(TextureSlot - TextureUnit.Texture0),
                0,
                0, false, 0,
                Access,
                Format);
        }
    }
}
