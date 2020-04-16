using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Osc.Net
{
    [XmlRoot("TransportConfigurationSettings")]
    public class TransportConfigurationCollection : KeyedCollection<string, TransportConfiguration>
    {
        protected override string GetKeyForItem(TransportConfiguration item)
        {
            return item.Name;
        }
    }
}
