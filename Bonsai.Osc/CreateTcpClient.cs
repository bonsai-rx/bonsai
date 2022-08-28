using Bonsai.Osc.Net;
using System.ComponentModel;

namespace Bonsai.Osc
{
    /// <summary>
    /// Represents an operator that creates an OSC communication client over TCP.
    /// </summary>
    [Description("Creates an OSC communication client over TCP.")]
    public class CreateTcpClient : CreateTransport
    {
        readonly TcpClientConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTcpClient"/> class.
        /// </summary>
        public CreateTcpClient()
            : this(new TcpClientConfiguration())
        {
        }

        private CreateTcpClient(TcpClientConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the DNS name of the remote host to which you intend
        /// to connect.
        /// </summary>
        [Description("The DNS name of the remote host to which you intend to connect.")]
        public string HostName
        {
            get { return configuration.HostName; }
            set { configuration.HostName = value; }
        }

        /// <summary>
        /// Gets or sets the port number of the remote host to which you intend
        /// to connect.
        /// </summary>
        [Description("The port number of the remote host to which you intend to connect.")]
        public int Port
        {
            get { return configuration.Port; }
            set { configuration.Port = value; }
        }

        /// <summary>
        /// Gets or sets a value that disables a delay when send or receive buffers
        /// are not full.
        /// </summary>
        [Description("If set to true, disables a delay when send or receive buffers are not full.")]
        public bool NoDelay
        {
            get { return configuration.NoDelay; }
            set { configuration.NoDelay = value; }
        }
    }
}
