using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Represents a collection of scene configuration objects.
    /// </summary>
    public class SceneConfigurationCollection : KeyedCollection<string, SceneConfiguration>
    {
        /// <summary>
        /// Returns the key for the specified configuration object.
        /// </summary>
        /// <param name="item">The configuration object from which to extract the key.</param>
        /// <returns>The key for the specified configuration object.</returns>
        protected override string GetKeyForItem(SceneConfiguration item)
        {
            return item.Name;
        }
    }
}
