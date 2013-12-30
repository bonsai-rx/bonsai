using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a collection of dynamic property mappings.
    /// </summary>
    public class PropertyMappingCollection : KeyedCollection<string, PropertyMapping>
    {
        /// <summary>
        /// Extracts the key from the specified property mapping.
        /// </summary>
        /// <param name="item">The property mapping from which to extract the key.</param>
        /// <returns>
        /// The key for the specified property mapping. The current key is the name of
        /// the property mapping.
        /// </returns>
        protected override string GetKeyForItem(PropertyMapping item)
        {
            return item.Name;
        }
    }
}
