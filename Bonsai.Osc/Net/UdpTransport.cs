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
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    class UdpTransport : ITransport
    {
        UdpClient client;
        IObservable<Message> messageReceived;

        public UdpTransport(UdpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.client = client;
            messageReceived = Observable.Using(
                () => new EventLoopScheduler(),
                scheduler => Observable.Create<Message>(observer =>
                {
                    var dispatcher = new Dispatcher(observer, scheduler);
                    return scheduler.Schedule(recurse =>
                    {
                        try
                        {
                            var endPoint = new IPEndPoint(IPAddress.Any, 0);
                            var packet = client.Receive(ref endPoint);
                            dispatcher.ProcessPacket(packet);
                            recurse();
                        }
                        catch (Exception e)
                        {
                            observer.OnError(e);
                        }
                    });
                }))
                .PublishReconnectable()
                .RefCount();
        }

        public IObservable<Message> MessageReceived
        {
            get { return messageReceived; }
        }

        public void SendPacket(Action<BigEndianWriter> writePacket)
        {
            byte[] buffer;
            using (var stream = new MemoryStream())
            using (var writer = new BigEndianWriter(stream))
            {
                writePacket(writer);
                buffer = stream.ToArray();
            }

            lock (client)
            {
                client.Send(buffer, buffer.Length);
            }
        }

        ~UdpTransport()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            var disposable = Interlocked.Exchange(ref client, null);
            if (disposable != null && disposing)
            {
                disposable.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
