using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Arduino
{
    [XmlRoot("ArduinoConfigurationSettings")]
    public class ArduinoConfigurationCollection : KeyedCollection<string, ArduinoConfiguration>
    {
        protected override string GetKeyForItem(ArduinoConfiguration item)
        {
            return item.PortName;
        }
    }
}
