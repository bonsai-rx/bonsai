using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    static class ObservableSerialPort
    {
        public static IObservable<string> ReadLine(string portName, string newLine)
        {
            return Observable.Create<string>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var data = string.Empty;
                    using var connection = SerialPortManager.ReserveConnection(portName);
                    using var cancellation = cancellationToken.Register(connection.Dispose);
                    var serialPort = connection.SerialPort;
                    if (string.IsNullOrEmpty(newLine)) newLine = serialPort.NewLine;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var bytesToRead = serialPort.BytesToRead;
                            if (bytesToRead == 0)
                            {
                                var next = (char)serialPort.ReadChar();
                                data = string.Concat(data, next, serialPort.ReadExisting());
                            }
                            else data = string.Concat(data, serialPort.ReadExisting());
                            if (cancellationToken.IsCancellationRequested) break;

                            var lines = data.Split(new[] { newLine }, StringSplitOptions.None);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (i == lines.Length - 1) data = lines[i];
                                else observer.OnNext(lines[i]);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                observer.OnError(ex);
                            }

                            break;
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        public static IObservable<TSource> WriteLine<TSource>(IObservable<TSource> source, string portName, string newLine)
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(portName),
                connection =>
                {
                    if (string.IsNullOrEmpty(newLine)) newLine = connection.SerialPort.NewLine;
                    return source.Do(value =>
                    {
                        connection.SerialPort.Write(value.ToString());
                        connection.SerialPort.Write(newLine);
                    });
                });
        }
    }
}
