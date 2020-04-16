using System.Net;
using System.Net.Sockets;

namespace Bonsai.Osc.Net
{
    public class TcpServerConfiguration : TransportConfiguration
    {
        public int Port { get; set; }

        public bool NoDelay { get; set; }

        public bool AllowNatTraversal { get; set; }

        internal override ITransport CreateTransport()
        {
            var listener = new TcpListener(IPAddress.Loopback, Port);
            listener.AllowNatTraversal(AllowNatTraversal);
            return new TcpServerTransport(listener, NoDelay);
        }
    }
}
