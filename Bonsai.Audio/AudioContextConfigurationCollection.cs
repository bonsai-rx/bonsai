using System.Collections.ObjectModel;
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
