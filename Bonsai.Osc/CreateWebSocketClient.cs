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
    [Description("Creates an Open Sound Control communication client over web socket.")]
    public class CreateWebSocketClient : CreateTransport
    {
        readonly WebSocketClientConfiguration configuration;

        public CreateWebSocketClient()
            : this(new WebSocketClientConfiguration())
        {
        }

        private CreateWebSocketClient(WebSocketClientConfiguration configuration)
            : base(configuration)
        {
            this.configuration = configuration;
        }

        [Description("The URI of the remote host to which you intend to connect.")]
        public string Uri
        {
            get => configuration.Uri;
            set => configuration.Uri = value; 
        }
    }
}
