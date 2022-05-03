using Bonsai.Resources;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides the abstract base class for configuring and loading texture resources.
    /// </summary>
    [XmlInclude(typeof(Cubemap))]
    [XmlInclude(typeof(Texture2D))]
    [XmlInclude(typeof(ImageTexture))]
    [XmlInclude(typeof(ImageCubemap))]
    [XmlInclude(typeof(ImageSequence))]
    [XmlInclude(typeof(VideoTexture))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class TextureConfiguration : ResourceConfiguration<Texture>
    {
    }
}
