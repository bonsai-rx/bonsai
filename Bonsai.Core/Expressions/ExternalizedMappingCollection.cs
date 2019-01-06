using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a collection of externalized property mappings.
    /// </summary>
    public class ExternalizedMappingCollection : KeyedCollection<string, ExternalizedMapping>
    {
        /// <summary>
        /// Extracts the key from the specified externalized property.
        /// </summary>
        /// <param name="item">The externalized property from which to extract the key.</param>
        /// <returns>
        /// The key for the specified externalized property. The current key is the name of
        /// the externalized property.
        /// </returns>
        protected override string GetKeyForItem(ExternalizedMapping item)
        {
            return item.Name;
        }
    }
}
