using Bonsai.Resources;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides an abstract base class for binding uniform variables to buffer objects.
    /// </summary>
    [XmlInclude(typeof(MeshBindingConfiguration))]
    [XmlInclude(typeof(TextureBindingConfiguration))]
    [XmlInclude(typeof(ImageTextureBindingConfiguration))]
    [XmlType(TypeName = "BufferBinding", Namespace = Constants.XmlNamespace)]
    public abstract class BufferBindingConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the uniform variable that will be bound to
        /// the buffer object.
        /// </summary>
        [Description("The name of the uniform variable that will be bound to the buffer object.")]
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
                return $"{baseText}({name})";
            }
            else return $"{baseText}({name} : {referenceName})";
        }
    }
}
