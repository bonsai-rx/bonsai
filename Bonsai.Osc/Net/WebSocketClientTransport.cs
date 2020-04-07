using Bonsai.Osc.IO;
using System;
using System.Drawing;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    class WebSocketClientTransport : ITransport
    {
        ClientWebSocket owner;
        readonly ManualResetEvent initialized = new ManualResetEvent(false);

        public WebSocketClientTransport(ClientWebSocket client, Uri uri)
        {
            owner = client;
            MessageReceived = Observable
                .FromAsync(cancellationToken => owner.ConnectAsync(uri, cancellationToken))
                .SelectMany(_ => Observable.Create<Message>(async (observer, cancellationToken) =>
                {
                    initialized.Set();

                    var pipe = new Pipe();
                    try
                    {
                        var writing = WebSocketClientPipe.GenerateAsync(client, 64, pipe.Writer, cancellationToken);
                        var reading = OscPipe.TransformAsync(pipe.Reader, observer, cancellationToken);

                        await Task.WhenAll(reading, writing);
                    }
                    finally
                    {
                        observer.OnCompleted();
                    }

                    return Disposable.Empty;
                }))
                .PublishReconnectable()
                .RefCount();
        }

        public IObservable<Message> MessageReceived { get; }

        public void SendPacket(Action<BigEndianWriter> writePacket)
        {
            initialized.WaitOne();

            byte[] payloadBuffer;
            using (var memoryStream = new MemoryStream())
            using (var writer = new BigEndianWriter(memoryStream))
            {
                writePacket(writer);
                payloadBuffer = memoryStream.ToArray();
            }

            var sizeBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payloadBuffer.Length));
            owner.SendAsync(new ArraySegment<byte>(sizeBuffer), WebSocketMessageType.Binary, false, CancellationToken.None).GetAwaiter().GetResult();
            owner.SendAsync(new ArraySegment<byte>(payloadBuffer), WebSocketMessageType.Binary, true, CancellationToken.None).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref owner, null);
            disposable?.Dispose();
        }
    }
}
