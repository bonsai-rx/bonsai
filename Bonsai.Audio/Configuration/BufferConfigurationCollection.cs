using System.Collections.ObjectModel;

namespace Bonsai.Audio.Configuration
{
    public class BufferConfigurationCollection : KeyedCollection<string, BufferConfiguration>
    {
        protected override string GetKeyForItem(BufferConfiguration item)
        {
            return item.Name;
        }
    }
}
