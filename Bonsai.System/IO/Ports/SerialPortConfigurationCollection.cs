using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.IO
{
    [Obsolete]
    [XmlRoot("SerialPortConfigurationSettings")]
    public class SerialPortConfigurationCollection : KeyedCollection<string, SerialPortConfiguration>
    {
        protected override string GetKeyForItem(SerialPortConfiguration item)
        {
            return item.PortName;
        }
    }
}
