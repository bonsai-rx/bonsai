using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio.Configuration
{
    public class BufferConfigurationCollection : KeyedCollection<string, BufferConfiguration>
    {
        protected override string GetKeyForItem(BufferConfiguration item)
        {
            return item.Name;
        }
    }
}
