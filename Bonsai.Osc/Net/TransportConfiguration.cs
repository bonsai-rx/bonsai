using System.Xml.Serialization;

namespace Bonsai.Osc.Net
{
    /// <summary>
    /// Provides an abstract base class for configuring the transport communication
    /// channel used by the OSC protocol.
    /// </summary>
    [XmlInclude(typeof(UdpConfiguration))]
    [XmlInclude(typeof(TcpClientConfiguration))]
    [XmlInclude(typeof(TcpServerConfiguration))]
    [XmlInclude(typeof(WebSocketClientConfiguration))]
    [XmlInclude(typeof(WebSocketServerConfiguration))]
    public abstract class TransportConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the communication channel to reserve
        /// for the OSC protocol.
        /// </summary>
        public string Name { get; set; }

        internal abstract ITransport CreateTransport();
    }
}
