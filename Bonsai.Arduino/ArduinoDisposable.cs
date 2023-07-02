using System;
using System.Threading;
using System.Reactive.Disposables;

namespace Bonsai.Arduino
{
    internal sealed class ArduinoDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public ArduinoDisposable(Arduino arduino, IDisposable disposable)
        {
            Arduino = arduino ?? throw new ArgumentNullException(nameof(arduino));
            resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
        }

        public Arduino Arduino { get; private set; }

        public bool IsDisposed
        {
            get { return resource == null; }
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref resource, null);
            if (disposable != null)
            {
                lock (ArduinoManager.SyncRoot)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
