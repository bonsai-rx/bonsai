using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    public class TcpServerConfiguration : TransportConfiguration
    {
        public int Port { get; set; }

        public bool NoDelay { get; set; }

        internal override ITransport CreateTransport()
        {
            var listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();
            var tcpClient = listener.AcceptTcpClient();
            tcpClient.NoDelay = NoDelay;
            return new TcpTransport(tcpClient);
        }
    }
}
