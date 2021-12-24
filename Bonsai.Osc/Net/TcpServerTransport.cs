using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Bonsai.Osc.Net
{
    class TcpServerTransport : ITransport
    {
        IDisposable subscription;
        readonly TcpListener owner;
        readonly object connectionsLock = new object();
        readonly List<TcpTransport> connections;
        readonly Subject<Message> messageReceived;

        public TcpServerTransport(TcpListener listener, bool noDelay)
        {
            listener.Start();
            owner = listener;
            connections = new List<TcpTransport>();
            messageReceived = new Subject<Message>();
            subscription = Observable
                .FromAsync(owner.AcceptTcpClientAsync)
                .Repeat()
                .Do(client => client.NoDelay = noDelay)
                .SelectMany(client => Observable.Using(
                    () => new TcpTransport(client),
                    transport =>
                    {
                        lock (connectionsLock) { connections.Add(transport); }
                        return transport.MessageReceived.Finally(() =>
                        {
                            lock (connectionsLock) { connections.Remove(transport); }
                            client.Dispose();
                        });
                    }))
                .Subscribe(messageReceived);
        }

        public IObservable<Message> MessageReceived
        {
            get { return messageReceived; }
        }

        public void SendPacket(Action<BigEndianWriter> writePacket)
        {
            lock (connectionsLock)
            {
                connections.RemoveAll(connection =>
                {
                    try
                    {
                        connection.SendPacket(writePacket);
                        return false;
                    }
                    catch (Exception ex)
                    when (ex is IOException ||
                          ex is ObjectDisposedException ||
                          ex is ArgumentException)
                    {
                        return true;
                    }
                });
            }
        }

        private void Dispose(bool disposing)
        {
            var disposable = Interlocked.Exchange(ref subscription, null);
            if (disposable != null && disposing)
            {
                disposable.Dispose();
                messageReceived.Dispose();
                owner.Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
