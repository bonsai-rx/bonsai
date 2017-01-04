using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(TypeName = "BufferBinding")]
    [XmlInclude(typeof(TextureBindingConfiguration))]
    [XmlInclude(typeof(ImageTextureBindingConfiguration))]
    [XmlInclude(typeof(MeshBindingConfiguration))]
    public abstract class BufferBindingConfiguration
    {
        [Description("The name of the uniform variable that will be bound to the buffer.")]
        public string Name { get; set; }

        internal abstract BufferBinding CreateBufferBinding();
    }
}
