using Bonsai.Resources;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlInclude(typeof(TexturedQuad))]
    [XmlInclude(typeof(TexturedModel))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class MeshConfiguration : ResourceConfiguration<Mesh>
    {
        public override Mesh CreateResource(ResourceManager resourceManager)
        {
            return new Mesh();
        }
    }
}
