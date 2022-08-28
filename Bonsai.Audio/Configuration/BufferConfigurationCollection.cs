using System.Collections.ObjectModel;

namespace Bonsai.Audio.Configuration
{
    /// <summary>
    /// Represents a collection of audio buffer resources.
    /// </summary>
    public class BufferConfigurationCollection : KeyedCollection<string, BufferConfiguration>
    {
        /// <inheritdoc/>
        protected override string GetKeyForItem(BufferConfiguration item)
        {
            return item.Name;
        }
    }
}
