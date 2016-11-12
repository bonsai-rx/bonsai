using OpenTK.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public sealed class AudioContextDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public AudioContextDisposable(AudioContext context, IDisposable disposable)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            Context = context;
            resource = disposable;
        }

        public AudioContext Context { get; private set; }

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
