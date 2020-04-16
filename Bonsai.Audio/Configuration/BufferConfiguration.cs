using Bonsai.Resources;
using System.Xml.Serialization;

namespace Bonsai.Audio.Configuration
{
    [XmlInclude(typeof(SoundBuffer))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class BufferConfiguration : ResourceConfiguration<Buffer>
    {
        public override Buffer CreateResource(ResourceManager resourceManager)
        {
            return new Buffer();
        }
    }
}
