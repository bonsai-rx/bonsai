using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reactive.Disposables;
using System.IO.Ports;

namespace Bonsai.IO
{
    public sealed class SerialPortDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public SerialPortDisposable(SerialPort serialPort, IDisposable disposable)
        {
            if (serialPort == null)
            {
                throw new ArgumentNullException("serialPort");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            SerialPort = serialPort;
            resource = disposable;
        }

        public SerialPort SerialPort { get; private set; }

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
