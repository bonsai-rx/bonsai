using System.Xml.Serialization;

namespace Bonsai.Osc.Net
{
    [XmlInclude(typeof(UdpConfiguration))]
    [XmlInclude(typeof(TcpClientConfiguration))]
    [XmlInclude(typeof(TcpServerConfiguration))]
    public abstract class TransportConfiguration
    {
        public string Name { get; set; }

        internal abstract ITransport CreateTransport();
    }
}
