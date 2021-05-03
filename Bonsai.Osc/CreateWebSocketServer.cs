using Bonsai.Osc.Net;
using System.ComponentModel;

namespace Bonsai.Osc
{
    [Description("Creates an Open Sound Control communication server over web socket.")]
    public class CreateWebSocketServer : CreateTransport
    {
        readonly WebSocketServerConfiguration configuration;

        public CreateWebSocketServer()
            : this(new WebSocketServerConfiguration())
        {
        }

        private CreateWebSocketServer(WebSocketServerConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        [Description("The port on which to listen for incoming connection attempts.")]
        public int Port
        {
            get => configuration.Port; 
            set => configuration.Port = value; 
        }

        [Description("If set to true, disables a delay when send or receive buffers are not full.")]
        public bool NoDelay
        {
            get => configuration.NoDelay; 
            set => configuration.NoDelay = value; 
        }

        [Description("Enables or disables Network Address Translation (NAT) on the server instance.")]
        public bool AllowNatTraversal
        {
            get => configuration.AllowNatTraversal; 
            set => configuration.AllowNatTraversal = value; 
        }
    }
}
