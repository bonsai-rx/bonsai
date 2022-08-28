using System;
using System.Threading;
using System.Reactive.Disposables;
using System.IO.Ports;

namespace Bonsai.IO
{
    internal sealed class SerialPortDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public SerialPortDisposable(SerialPort serialPort, IDisposable disposable)
        {
            SerialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
        }

        public SerialPort SerialPort { get; private set; }

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
