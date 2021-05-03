using System;
using System.Net.WebSockets;

namespace Bonsai.Osc.Net
{
    public class WebSocketClientConfiguration : TransportConfiguration
    {
        public string Uri { get; set; }

        internal override ITransport CreateTransport()
        {
            var client = new ClientWebSocket();
            return new WebSocketClientTransport(client, new Uri(Uri));
        }
    }
}
