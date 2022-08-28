using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai.Audio
{
    internal sealed class AudioContextDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public AudioContextDisposable(AudioContextManager context, IDisposable disposable)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
        }

        public AudioContextManager Context { get; private set; }

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
