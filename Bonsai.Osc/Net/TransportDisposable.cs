using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reactive.Disposables;
using System.Net.Sockets;

namespace Bonsai.Osc.Net
{
    public sealed class TransportDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        internal TransportDisposable(ITransport transport, IDisposable disposable)
        {
            if (transport == null)
            {
                throw new ArgumentNullException("transport");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            Transport = transport;
            resource = disposable;
        }

        internal ITransport Transport { get; private set; }

        public bool IsDisposed
        {
            get { return resource == null; }
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange<IDisposable>(ref resource, null);
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
