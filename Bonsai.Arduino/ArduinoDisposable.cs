using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bonsai.Arduino
{
    public sealed class ArduinoDisposable : IDisposable
    {
        int disposed;
        IDisposable disposable;

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
            this.disposable = disposable;
        }

        public Arduino Arduino { get; private set; }

        public bool IsDisposed
        {
            get { return disposed == 1; }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                disposable.Dispose();
            }
        }
    }
}
