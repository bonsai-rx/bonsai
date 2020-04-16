using System;
using System.Threading;
using System.Reactive.Disposables;

namespace Bonsai.Arduino
{
    public sealed class ArduinoDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public ArduinoDisposable(Arduino arduino, IDisposable disposable)
        {
            if (arduino == null)
            {
                throw new ArgumentNullException("arduino");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            Arduino = arduino;
            resource = disposable;
        }

        public Arduino Arduino { get; private set; }

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
