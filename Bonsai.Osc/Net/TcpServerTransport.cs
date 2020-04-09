using Bonsai.Osc.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    class TcpServerTransport : ITransport
    {
        TcpListener owner;
        readonly ConcurrentBag<TcpClientTransport> connections;
        readonly IObservable<Message> messageReceived;

        public TcpServerTransport(TcpListener listener, bool noDelay)
        {
            owner = listener;
            connections = new ConcurrentBag<TcpClientTransport>();
            messageReceived = Observable
                .FromAsync(listener.AcceptTcpClientAsync)
                .Repeat()
                .Do(client => client.NoDelay = noDelay)
                .SelectMany(client => Observable.Using(
                    () => new TcpClientTransport(client),
                    transport =>
                    {
                        connections.Add(transport);
                        return transport.MessageReceived.Finally(() =>
                        {
                            connections.TryTake(out _);
                        });
                    }));
        }

        public IObservable<Message> MessageReceived
        {
            get { return messageReceived; }
        }

        public void SendPacket(Action<BigEndianWriter> writePacket)
        {
            foreach (var connection in connections)
            {
                connection.SendPacket(writePacket);
            }
        }

        private void Dispose(bool disposing)
        {
            var disposable = Interlocked.Exchange(ref owner, null);
            if (disposable != null && disposing)
            {
                disposable.Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
