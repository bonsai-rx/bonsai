using System.Net;
using System.Net.Sockets;

namespace Bonsai.Osc.Net
{
    /// <summary>
    /// Provides settings for creating and configuring an OSC communication server
    /// over TCP.
    /// </summary>
    public class TcpServerConfiguration : TransportConfiguration
    {
        /// <summary>
        /// Gets or sets the port on which to listen for incoming connection attempts.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value that disables a delay when send or receive buffers
        /// are not full.
        /// </summary>
        public bool NoDelay { get; set; }

        /// <summary>
        /// Gets or sets a value that enables or disables Network Address
        /// Translation (NAT) traversal on the TCP server.
        /// </summary>
        public bool AllowNatTraversal { get; set; }

        internal override ITransport CreateTransport()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.AllowNatTraversal(AllowNatTraversal);
            return new TcpServerTransport(listener, NoDelay);
        }
    }
}
