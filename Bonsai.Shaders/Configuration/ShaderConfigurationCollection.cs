using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a collection of shader configuration objects.
    /// </summary>
    public class ShaderConfigurationCollection : KeyedCollection<string, ShaderConfiguration>
    {
        /// <summary>
        /// Returns the key for the specified configuration object.
        /// </summary>
        /// <param name="item">The configuration object from which to extract the key.</param>
        /// <returns>The key for the specified configuration object.</returns>
        protected override string GetKeyForItem(ShaderConfiguration item)
        {
            return item.Name;
        }
    }
}
