using System.Collections.ObjectModel;

namespace Bonsai.Audio.Configuration
{
    public class SourceConfigurationCollection : KeyedCollection<string, SourceConfiguration>
    {
        protected override string GetKeyForItem(SourceConfiguration item)
        {
            return item.Name;
        }
    }
}
