using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Rendering
{
    public class SceneConfigurationCollection : KeyedCollection<string, SceneConfiguration>
    {
        protected override string GetKeyForItem(SceneConfiguration item)
        {
            return item.Name;
        }
    }
}
