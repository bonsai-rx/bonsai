using Bonsai.Osc.IO;
using System;
using System.Collections.Immutable;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    class WebSocketServerTransport : ITransport
    {
        TcpListener owner;
        ImmutableDictionary<TcpClient, NetworkStream> connections = ImmutableDictionary<TcpClient, NetworkStream>.Empty;

        public WebSocketServerTransport(TcpListener listener, bool noDelay)
        {
            listener.Start();
            owner = listener;
            MessageReceived = Observable
                .FromAsync(owner.AcceptTcpClientAsync)
                .Repeat()
                .Do(client => client.NoDelay = noDelay)
                .SelectMany(client => Observable.Create<Message>(async (observer, cancellationToken) =>
                {
                    var stream = client.GetStream();
                    connections = connections.Add(client, stream);

                    var webSocketPipe = new Pipe();
                    var oscPipe = new Pipe();
                    try
                    {
                        var writing = TcpClientPipe.GenerateAsync(stream, 64, webSocketPipe.Writer, cancellationToken);
                        var transforming = WebSocketServerPipe.TransformAsync(webSocketPipe.Reader, stream, oscPipe.Writer, cancellationToken);
                        var reading = OscPipe.TransformAsync(oscPipe.Reader, observer, cancellationToken);

                        await Task.WhenAll(reading, transforming, writing);
                    }
                    finally
                    {
                        connections = connections.Remove(client);

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
            byte[] payloadBuffer;
            using (var memoryStream = new MemoryStream())
            using (var writer = new BigEndianWriter(memoryStream))
            {
                writePacket(writer);
                payloadBuffer = memoryStream.ToArray();
            }

            // prepend the size
            byte[] packetBuffer;
            using (var memoryStream = new MemoryStream())
            using (var writer = new BigEndianWriter(memoryStream))
            {
                writer.Write(payloadBuffer.Length);
                writer.Write(payloadBuffer);
                packetBuffer = memoryStream.ToArray();
            }

            var encoded = WebSocketServerPipe.Encode(new ArraySegment<byte>(packetBuffer), WebSocketServerPipe.Opcode.Binary); // encode as web socket frame

            foreach (var connection in connections.Values)
            {
                connection.WriteAsync(encoded, 0, encoded.Length).GetAwaiter().GetResult();
            }
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref owner, null);
            disposable?.Stop();
        }
    }
}
