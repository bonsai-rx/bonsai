using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO.Ports
{
    static class ObservableSerialPort
    {
        public static IObservable<byte[]> Read(string portName, int count)
        {
            return Observable.Create<byte[]>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using var connection = SerialPortManager.ReserveConnection(portName);
                    using var cancellation = cancellationToken.Register(connection.Dispose);
                    var serialPort = connection.SerialPort;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var bytesRead = 0;
                            var buffer = new byte[count];
                            while (bytesRead < count)
                            {
                                bytesRead += serialPort.Read(buffer, bytesRead, count - bytesRead);
                            }
                            observer.OnNext(buffer);
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

        public static IObservable<string> ReadLine(string portName, string newLine)
        {
            return Observable.Create<string>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using var connection = SerialPortManager.ReserveConnection(portName);
                    using var cancellation = cancellationToken.Register(connection.Dispose);
                    var serialPort = connection.SerialPort;
                    if (string.IsNullOrEmpty(newLine))
                    {
                        newLine = serialPort.NewLine;
                    }

                    var lineBuilder = new StringBuilder();
                    var lastChar = newLine[newLine.Length - 1];
                    var readBuffer = new char[1];
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var found = false;
                            while (!found)
                            {
                                var bytesRead = serialPort.Read(readBuffer, 0, 1);
                                lineBuilder.Append(readBuffer, 0, bytesRead);
                                if (readBuffer[0] != lastChar || lineBuilder.Length < newLine.Length)
                                {
                                    continue;
                                }

                                found = true;
                                for (int i = 2; i <= newLine.Length; i++)
                                {
                                    if (newLine[newLine.Length - i] != lineBuilder[lineBuilder.Length - i])
                                    {
                                        found = false;
                                        break;
                                    }
                                }
                            }

                            var result = lineBuilder.ToString(0, lineBuilder.Length - newLine.Length);
                            observer.OnNext(result);
                            lineBuilder.Clear();
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
                    if (string.IsNullOrEmpty(newLine))
                    {
                        newLine = connection.SerialPort.NewLine;
                    }

                    return source.Do(value =>
                    {
                        connection.SerialPort.Write(value.ToString());
                        connection.SerialPort.Write(newLine);
                    });
                });
        }
    }
}
