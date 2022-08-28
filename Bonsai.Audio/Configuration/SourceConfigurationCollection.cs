using System.Collections.ObjectModel;

namespace Bonsai.Audio.Configuration
{
    /// <summary>
    /// Represents a collection of audio source resources.
    /// </summary>
    public class SourceConfigurationCollection : KeyedCollection<string, SourceConfiguration>
    {
        /// <inheritdoc/>
        protected override string GetKeyForItem(SourceConfiguration item)
        {
            return item.Name;
        }
    }
}
