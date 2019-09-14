using Bonsai.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlInclude(typeof(MeshBindingConfiguration))]
    [XmlInclude(typeof(TextureBindingConfiguration))]
    [XmlInclude(typeof(ImageTextureBindingConfiguration))]
    [XmlType(TypeName = "BufferBinding", Namespace = Constants.XmlNamespace)]
    public abstract class BufferBindingConfiguration
    {
        [Description("The name of the uniform variable that will be bound to the buffer.")]
        public string Name { get; set; }

        internal abstract BufferBinding CreateBufferBinding(Shader shader, ResourceManager resourceManager);

        internal string ToString(string baseText, string referenceName)
        {
            var name = Name;
            if (string.IsNullOrEmpty(name))
            {
                return GetType().Name;
            }
            else if (string.IsNullOrEmpty(referenceName))
            {
                return string.Format("{0}({1})", baseText, name);
            }
            else return string.Format("{0}({1} : {2})", baseText, name, referenceName);
        }
    }
}
