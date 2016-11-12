using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Audio
{
    [XmlRoot("AudioContextSettings")]
    public class AudioContextConfigurationCollection : KeyedCollection<string, AudioContextConfiguration>
    {
        protected override string GetKeyForItem(AudioContextConfiguration item)
        {
            return item.DeviceName;
        }
    }
}
