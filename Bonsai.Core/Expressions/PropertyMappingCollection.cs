using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Bonsai.Expressions
{
    public class PropertyMappingCollection : KeyedCollection<string, PropertyMapping>
    {
        protected override string GetKeyForItem(PropertyMapping item)
        {
            return item.Name;
        }
    }
}
