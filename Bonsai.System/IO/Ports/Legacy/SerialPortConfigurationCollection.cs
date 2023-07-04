using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents a collection of serial port configuration objects.
    /// </summary>
    [Obsolete]
    [XmlRoot("SerialPortConfigurationSettings")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SerialPortConfigurationCollection : KeyedCollection<string, SerialPortConfiguration>
    {
        /// <summary>
        /// Extracts the key from the specified <see cref="SerialPortConfiguration"/> object.
        /// </summary>
        /// <param name="item">The <see cref="SerialPortConfiguration"/> object from which to extract the key.</param>
        /// <returns>
        /// The key for the specified <see cref="SerialPortConfiguration"/> object. Currently,
        /// this is the name of the serial port associated with the configuration object.
        /// </returns>
        protected override string GetKeyForItem(SerialPortConfiguration item)
        {
            return item.PortName;
        }
    }
}
