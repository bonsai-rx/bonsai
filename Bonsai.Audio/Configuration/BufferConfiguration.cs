using Bonsai.Resources;
using System.Xml.Serialization;

namespace Bonsai.Audio.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for audio buffer resources.
    /// </summary>
    [XmlInclude(typeof(SoundBuffer))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class BufferConfiguration : ResourceConfiguration<Buffer>
    {
        /// <summary>
        /// Creates a new empty audio buffer resource, typically used for uploading dynamic data.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Buffer"/> class.
        /// </returns>
        /// <inheritdoc/>
        public override Buffer CreateResource(ResourceManager resourceManager)
        {
            return new Buffer();
        }
    }
}
