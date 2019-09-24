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
    [Description("Creates an Open Sound Control communication client over TCP.")]
    public class CreateTcpClient : CreateTransport
    {
        readonly TcpClientConfiguration configuration;

        public CreateTcpClient()
            : this(new TcpClientConfiguration())
        {
        }

        private CreateTcpClient(TcpClientConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        [Description("The DNS name of the remote host to which you intend to connect.")]
        public string HostName
        {
            get { return configuration.HostName; }
            set { configuration.HostName = value; }
        }

        [Description("The port number of the remote host to which you intend to connect.")]
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
