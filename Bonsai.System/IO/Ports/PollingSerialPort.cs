using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    class PollingSerialPort : SerialPort
    {
        int fd;
        object data_received;
        static readonly Func<SerialData, SerialDataReceivedEventArgs> newSerialDataReceivedEventArgs;
        CancellationTokenSource cancellationTokenSource;

        static PollingSerialPort()
        {
            newSerialDataReceivedEventArgs = CreateNewSerialEventArgs<SerialData, SerialDataReceivedEventArgs>();
        }

        public PollingSerialPort()
        {
        }

        public PollingSerialPort(IContainer container)
            : base(container)
        {
        }

        public PollingSerialPort(string portName)
            : base(portName)
        {
        }

        public PollingSerialPort(string portName, int baudRate)
            : base(portName, baudRate)
        {
        }

        public PollingSerialPort(string portName, int baudRate, Parity parity)
            : base(portName, baudRate, parity)
        {
        }

        public PollingSerialPort(string portName, int baudRate, Parity parity, int dataBits)
            : base(portName, baudRate, parity, dataBits)
        {
        }

        public PollingSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
        }

        static Func<TEventType, TEventArgs> CreateNewSerialEventArgs<TEventType, TEventArgs>()
        {
            var constructorInfo = typeof(TEventArgs).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                new[] { typeof(TEventType) },
                modifiers: null);
            var parameter = Expression.Parameter(typeof(TEventType));
            var body = Expression.New(constructorInfo, parameter);
            var lambda = Expression.Lambda<Func<TEventType, TEventArgs>>(body, parameter);
            return lambda.Compile();
        }

        public new void Open()
        {
            base.Open();
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var fdField = BaseStream.GetType().GetField(nameof(fd), bindingFlags);
            var dataReceivedField = typeof(SerialPort).GetField(nameof(data_received), bindingFlags);
            data_received = dataReceivedField?.GetValue(this);
            if (fdField != null)
            {
                fd = (int)fdField.GetValue(BaseStream);
            }

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (PollSerialStream(ReadTimeout))
                    {
                        OnDataReceived(newSerialDataReceivedEventArgs(SerialData.Chars));
                    }
                }
            },
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        }

        private void OnDataReceived(SerialDataReceivedEventArgs e)
        {
            ((SerialDataReceivedEventHandler)Events[data_received])?.Invoke(this, e);
        }

        private bool PollSerialStream(int timeout)
        {
            var result = poll_serial(fd, out int error, timeout);
            if (error < 0)
            {
                ThrowIOException();
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cancellationTokenSource.Dispose();
            }

            base.Dispose(disposing);
        }

        [DllImport("MonoPosixHelper", SetLastError = true)]
        static extern bool poll_serial(int fd, out int error, int timeout);

        [DllImport("libc")]
        static extern IntPtr strerror(int errnum);

        static void ThrowIOException()
        {
            int errnum = Marshal.GetLastWin32Error();
            string error_message = Marshal.PtrToStringAnsi(strerror(errnum));

            throw new IOException(error_message);
        }
    }
}
