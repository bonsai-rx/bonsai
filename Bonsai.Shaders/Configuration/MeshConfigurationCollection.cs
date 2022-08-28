using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a collection of mesh configuration objects.
    /// </summary>
    public class MeshConfigurationCollection : KeyedCollection<string, MeshConfiguration>
    {
        /// <summary>
        /// Returns the key for the specified configuration object.
        /// </summary>
        /// <param name="item">The configuration object from which to extract the key.</param>
        /// <returns>The key for the specified configuration object.</returns>
        protected override string GetKeyForItem(MeshConfiguration item)
        {
            return item.Name;
        }
    }
}
