using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    public class TcpClientConfiguration : TransportConfiguration
    {
        public string HostName { get; set; }

        public int Port { get; set; }

        public bool NoDelay { get; set; }

        internal override ITransport CreateTransport()
        {
            var tcpClient = new TcpClient();
            tcpClient.NoDelay = NoDelay;
            tcpClient.Connect(HostName, Port);
            return new TcpTransport(tcpClient);
        }
    }
}
