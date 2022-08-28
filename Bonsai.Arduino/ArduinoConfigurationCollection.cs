using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents a collection of Firmata configuration settings, indexed by port name.
    /// </summary>
    [Obsolete]
    [XmlRoot("ArduinoConfigurationSettings")]
    public class ArduinoConfigurationCollection : KeyedCollection<string, ArduinoConfiguration>
    {
        /// <inheritdoc/>
        protected override string GetKeyForItem(ArduinoConfiguration item)
        {
            return item.PortName;
        }
    }
}
