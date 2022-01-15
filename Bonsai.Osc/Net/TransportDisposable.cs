using System;
using System.Threading;
using System.Reactive.Disposables;

namespace Bonsai.Osc.Net
{
    internal sealed class TransportDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        internal TransportDisposable(ITransport transport, IDisposable disposable)
        {
            Transport = transport ?? throw new ArgumentNullException(nameof(transport));
            resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
        }

        internal ITransport Transport { get; private set; }

        public bool IsDisposed
        {
            get { return resource == null; }
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref resource, null);
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
