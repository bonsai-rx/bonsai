using Bonsai.Osc.Net;
using System.ComponentModel;

namespace Bonsai.Osc
{
    /// <summary>
    /// Represents an operator that creates an OSC communication server over TCP.
    /// </summary>
    [Description("Creates an OSC communication server over TCP.")]
    public class CreateTcpServer : CreateTransport
    {
        readonly TcpServerConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTcpServer"/> class.
        /// </summary>
        public CreateTcpServer()
            : this(new TcpServerConfiguration())
        {
        }

        private CreateTcpServer(TcpServerConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the port on which to listen for incoming connection attempts.
        /// </summary>
        [Description("The port on which to listen for incoming connection attempts.")]
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

        /// <summary>
        /// Gets or sets a value that enables or disables Network Address
        /// Translation (NAT) traversal on the TCP server.
        /// </summary>
        [Description("Enables or disables Network Address Translation (NAT) on the TCP server.")]
        public bool AllowNatTraversal
        {
            get { return configuration.AllowNatTraversal; }
            set { configuration.AllowNatTraversal = value; }
        }
    }
}
