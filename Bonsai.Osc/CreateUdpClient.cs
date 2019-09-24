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
    [Description("Creates an Open Sound Control communication channel over UDP.")]
    public class CreateUdpClient : CreateTransport
    {
        readonly UdpConfiguration configuration;

        public CreateUdpClient()
            : this(new UdpConfiguration())
        {
        }

        private CreateUdpClient(UdpConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        [Description("The local port number from which you intend to communicate.")]
        public int Port
        {
            get { return configuration.Port; }
            set { configuration.Port = value; }
        }

        [Description("The DNS name of the remote host to send data to or receive data from. If empty, the channel will accept connections from any host.")]
        public string RemoteHostName
        {
            get { return configuration.RemoteHostName; }
            set { configuration.RemoteHostName = value; }
        }

        [Description("The port number on the remote host to send data to or receive data from. If left to zero, the channel will accept connections from any port.")]
        public int RemotePort
        {
            get { return configuration.RemotePort; }
            set { configuration.RemotePort = value; }
        }
    }
}
