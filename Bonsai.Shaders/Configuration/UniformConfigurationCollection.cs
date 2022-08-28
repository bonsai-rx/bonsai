using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a collection of configuration objects used to initialize the
    /// values of uniform variables in a shader program.
    /// </summary>
    public class UniformConfigurationCollection : KeyedCollection<string, UniformConfiguration>
    {
        /// <summary>
        /// Returns the key for the specified configuration object.
        /// </summary>
        /// <param name="item">The configuration object from which to extract the key.</param>
        /// <returns>The key for the specified configuration object.</returns>
        protected override string GetKeyForItem(UniformConfiguration item)
        {
            return item.Name;
        }
    }
}
