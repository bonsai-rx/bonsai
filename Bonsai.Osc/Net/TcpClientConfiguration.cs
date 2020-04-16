using System.Net.Sockets;

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
            return new TcpClientTransport(tcpClient, HostName, Port);
        }
    }
}
