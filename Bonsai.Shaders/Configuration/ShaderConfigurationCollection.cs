using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    public class ShaderConfigurationCollection : KeyedCollection<string, ShaderConfiguration>
    {
        protected override string GetKeyForItem(ShaderConfiguration item)
        {
            return item.Name;
        }
    }
}
