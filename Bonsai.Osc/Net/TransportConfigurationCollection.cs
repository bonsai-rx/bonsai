using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Osc.Net
{
    /// <summary>
    /// Represents a collection of transport configuration settings, indexed by connection name.
    /// </summary>
    [Obsolete]
    [XmlRoot("TransportConfigurationSettings")]
    public class TransportConfigurationCollection : KeyedCollection<string, TransportConfiguration>
    {
        /// <inheritdoc/>
        protected override string GetKeyForItem(TransportConfiguration item)
        {
            return item.Name;
        }
    }
}
