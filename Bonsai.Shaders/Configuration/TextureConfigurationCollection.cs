using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    public class TextureConfigurationCollection : KeyedCollection<string, TextureConfiguration>
    {
        protected override string GetKeyForItem(TextureConfiguration item)
        {
            return item.Name;
        }
    }
}
