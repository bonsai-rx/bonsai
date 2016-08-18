using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class UniformConfigurationCollection : KeyedCollection<string, UniformConfiguration>
    {
        protected override string GetKeyForItem(UniformConfiguration item)
        {
            return item.Name;
        }
    }
}
