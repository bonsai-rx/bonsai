using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a collection of buffer binding configuration objects.
    /// </summary>
    public class BufferBindingConfigurationCollection : KeyedCollection<string, BufferBindingConfiguration>
    {
        /// <summary>
        /// Returns the key for the specified configuration object.
        /// </summary>
        /// <param name="item">The configuration object from which to extract the key.</param>
        /// <returns>The key for the specified configuration object.</returns>
        protected override string GetKeyForItem(BufferBindingConfiguration item)
        {
            return item.Name;
        }
    }
}
