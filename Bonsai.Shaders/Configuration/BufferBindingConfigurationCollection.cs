using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    public class BufferBindingConfigurationCollection : KeyedCollection<string, BufferBindingConfiguration>
    {
        protected override string GetKeyForItem(BufferBindingConfiguration item)
        {
            return item.Name;
        }
    }
}
