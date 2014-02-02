using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
