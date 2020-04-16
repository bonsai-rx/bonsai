using Bonsai.Osc.IO;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Bonsai.Osc.Net
{
    class TcpClientTransport : ITransport
    {
        IDisposable subscription;
        TcpTransport connection;
        readonly TcpClient owner;
        readonly ManualResetEvent initialized;
        readonly Subject<Message> messageReceived;

        public TcpClientTransport(TcpClient client, string host, int port)
        {
            owner = client;
            initialized = new ManualResetEvent(false);
            messageReceived = new Subject<Message>();
            subscription = Observable
                .FromAsync(() => client.ConnectAsync(host, port))
                .SelectMany(unit => Observable.Using(
                    () => new TcpTransport(client),
                    transport =>
                    {
                        Interlocked.Exchange(ref connection, transport);
                        initialized.Set();
                        return transport.MessageReceived;
                    }))
                .Subscribe(messageReceived);
        }

        public IObservable<Message> MessageReceived
        {
            get { return messageReceived; }
        }

        public void SendPacket(Action<BigEndianWriter> writePacket)
        {
            if (connection == null)
            {
                initialized.WaitOne();
            }

            connection.SendPacket(writePacket);
        }

        private void Dispose(bool disposing)
        {
            var disposable = Interlocked.Exchange(ref subscription, null);
            if (disposable != null && disposing)
            {
                disposable.Dispose();
                messageReceived.Dispose();
                owner.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
