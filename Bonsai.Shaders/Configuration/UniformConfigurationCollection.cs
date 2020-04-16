using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    public class UniformConfigurationCollection : KeyedCollection<string, UniformConfiguration>
    {
        protected override string GetKeyForItem(UniformConfiguration item)
        {
            return item.Name;
        }
    }
}
