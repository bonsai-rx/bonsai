using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    public class MaterialConfigurationCollection : KeyedCollection<string, MaterialConfiguration>
    {
        protected override string GetKeyForItem(MaterialConfiguration item)
        {
            return item.Name;
        }
    }
}
