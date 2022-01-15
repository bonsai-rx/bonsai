using System.Net.Sockets;

namespace Bonsai.Osc.Net
{
    /// <summary>
    /// Provides settings for configuring an OSC communication channel over UDP.
    /// </summary>
    public class UdpConfiguration : TransportConfiguration
    {
        /// <summary>
        /// Gets or sets the local port number from which you intend to communicate.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the DNS name of the remote host to which you intend
        /// to send data.
        /// </summary>
        public string RemoteHostName { get; set; }

        /// <summary>
        /// Gets or sets the port number on the remote host to which you intend
        /// to send data.
        /// </summary>
        public int RemotePort { get; set; }

        internal override ITransport CreateTransport()
        {
            var udpClient = new UdpClient(Port);
            if (RemotePort > 0) udpClient.Connect(RemoteHostName, RemotePort);
            return new UdpTransport(udpClient);
        }
    }
}
