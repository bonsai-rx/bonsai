using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio.Configuration
{
    public class SourceConfigurationCollection : KeyedCollection<string, SourceConfiguration>
    {
        protected override string GetKeyForItem(SourceConfiguration item)
        {
            return item.Name;
        }
    }
}
