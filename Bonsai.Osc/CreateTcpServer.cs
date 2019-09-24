using Bonsai.Osc.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    [Description("Creates an Open Sound Control communication server over TCP.")]
    public class CreateTcpServer : CreateTransport
    {
        readonly TcpServerConfiguration configuration;

        public CreateTcpServer()
            : this(new TcpServerConfiguration())
        {
        }

        private CreateTcpServer(TcpServerConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        [Description("The port on which to listen for incoming connection attempts.")]
        public int Port
        {
            get { return configuration.Port; }
            set { configuration.Port = value; }
        }

        [Description("If set to true, disables a delay when send or receive buffers are not full.")]
        public bool NoDelay
        {
            get { return configuration.NoDelay; }
            set { configuration.NoDelay = value; }
        }
    }
}
