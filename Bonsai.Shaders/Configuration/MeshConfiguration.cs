using Bonsai.Resources;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for mesh resources.
    /// </summary>
    [XmlInclude(typeof(TexturedQuad))]
    [XmlInclude(typeof(TexturedModel))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class MeshConfiguration : ResourceConfiguration<Mesh>
    {
        /// <summary>
        /// Creates a new empty mesh resource, typically used for uploading dynamic
        /// geometry data.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Mesh"/> class.
        /// </returns>
        /// <inheritdoc/>
        public override Mesh CreateResource(ResourceManager resourceManager)
        {
            return new Mesh();
        }
    }
}
