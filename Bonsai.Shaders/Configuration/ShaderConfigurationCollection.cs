using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    public class ShaderConfigurationCollection : KeyedCollection<string, ShaderConfiguration>
    {
        protected override string GetKeyForItem(ShaderConfiguration item)
        {
            return item.Name;
        }
    }
}
