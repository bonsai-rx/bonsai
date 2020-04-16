using Bonsai.Resources;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlInclude(typeof(Cubemap))]
    [XmlInclude(typeof(Texture2D))]
    [XmlInclude(typeof(ImageTexture))]
    [XmlInclude(typeof(ImageCubemap))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class TextureConfiguration : ResourceConfiguration<Texture>
    {
    }
}
