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
    [XmlType(TypeName = "ImageTextureBinding")]
    public class ImageTextureBindingConfiguration : TextureBindingConfiguration
    {
        public ImageTextureBindingConfiguration()
        {
            Format = SizedInternalFormat.Rgba32f;
            Access = TextureAccess.ReadOnly;
        }

        [Description("The type of access that will be performed on the image.")]
        public TextureAccess Access { get; set; }

        [Description("The format that the elements of the image will be treated as for the purposes of formatted stores.")]
        public SizedInternalFormat Format { get; set; }

        internal override BufferBinding CreateBufferBinding()
        {
            return new ImageTextureBinding(this);
        }
    }
}
