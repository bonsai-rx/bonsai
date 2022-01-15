using System.Net.Sockets;

namespace Bonsai.Osc.Net
{
    /// <summary>
    /// Provides settings for configuring an OSC communication client over TCP.
    /// </summary>
    public class TcpClientConfiguration : TransportConfiguration
    {
        /// <summary>
        /// Gets or sets the DNS name of the remote host to which you intend
        /// to connect.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the port number of the remote host to which you intend
        /// to connect.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value that disables a delay when send or receive buffers
        /// are not full.
        /// </summary>
        public bool NoDelay { get; set; }

        internal override ITransport CreateTransport()
        {
            var tcpClient = new TcpClient();
            tcpClient.NoDelay = NoDelay;
            return new TcpClientTransport(tcpClient, HostName, Port);
        }
    }
}
