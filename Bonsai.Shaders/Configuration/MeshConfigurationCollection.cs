using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    public class MeshConfigurationCollection : KeyedCollection<string, MeshConfiguration>
    {
        protected override string GetKeyForItem(MeshConfiguration item)
        {
            return item.Name;
        }
    }
}
