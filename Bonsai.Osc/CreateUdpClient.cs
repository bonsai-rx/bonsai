using Bonsai.Osc.Net;
using System.ComponentModel;

namespace Bonsai.Osc
{
    /// <summary>
    /// Represents an operator that creates an OSC communication channel over UDP.
    /// </summary>
    [Description("Creates an OSC communication channel over UDP.")]
    public class CreateUdpClient : CreateTransport
    {
        readonly UdpConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateUdpClient"/> class.
        /// </summary>
        public CreateUdpClient()
            : this(new UdpConfiguration())
        {
        }

        private CreateUdpClient(UdpConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the local port number from which you intend to communicate.
        /// </summary>
        [Description("The local port number from which you intend to communicate.")]
        public int Port
        {
            get { return configuration.Port; }
            set { configuration.Port = value; }
        }

        /// <summary>
        /// Gets or sets the DNS name of the remote host to which you intend
        /// to send data. If empty, the channel will accept connections from any host.
        /// </summary>
        [Description("The DNS name of the remote host to send data to or receive data from. If empty, the channel will accept connections from any host.")]
        public string RemoteHostName
        {
            get { return configuration.RemoteHostName; }
            set { configuration.RemoteHostName = value; }
        }

        /// <summary>
        /// Gets or sets the port number on the remote host to which you intend
        /// to send data. If this value is zero, the channel will accept connections
        /// from any port.
        /// </summary>
        [Description("The port number on the remote host to send data to or receive data from. If this value is zero, the channel will accept connections from any port.")]
        public int RemotePort
        {
            get { return configuration.RemotePort; }
            set { configuration.RemotePort = value; }
        }
    }
}
