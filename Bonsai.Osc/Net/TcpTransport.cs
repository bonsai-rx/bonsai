using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    class TcpTransport : ITransport
    {
        NetworkStream stream;
        IObservable<Message> messageReceived;

        public TcpTransport(TcpClient client)
        {
            stream = client.GetStream();
            messageReceived = Observable.Create<Message>(observer =>
            {
                var sizeBuffer = new byte[sizeof(int)];
                var scheduler = new EventLoopScheduler();
                var dispatcher = new Dispatcher(observer, scheduler);
                return scheduler.Schedule(recurse =>
                {
                    var bytesRead = stream.Read(sizeBuffer, 0, sizeBuffer.Length);
                    if (bytesRead < sizeBuffer.Length)
                    {
                        observer.OnError(new InvalidOperationException("Unexpected end of stream."));
                        scheduler.Dispose();
                    }
                    else
                    {
                        var packetSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(sizeBuffer, 0));
                        var packet = new byte[packetSize];
                        bytesRead = stream.Read(packet, 0, packet.Length);
                        if (bytesRead < packet.Length)
                        {
                            observer.OnError(new InvalidOperationException("Unexpected end of stream."));
                            scheduler.Dispose();
                        }
                        else
                        {
                            dispatcher.ProcessPacket(packet);
                            recurse();
                        }
                    }
                });
            })
            .Publish()
            .RefCount();
        }

        public IObservable<Message> MessageReceived
        {
            get { return messageReceived; }
        }

        public void SendPacket(Action<BigEndianWriter> writePacket)
        {
            byte[] buffer;
            using (var memoryStream = new MemoryStream())
            using (var writer = new BigEndianWriter(memoryStream))
            {
                writePacket(writer);
                buffer = memoryStream.ToArray();
            }

            lock (stream)
            {
                using (var writer = new BigEndianWriter(stream, true))
                {
                    writer.Write(buffer.Length);
                    writer.Write(buffer);
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
