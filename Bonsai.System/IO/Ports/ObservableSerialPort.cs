using System;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    static class ObservableSerialPort
    {
        public const string DefaultNewLine = @"\r\n";

        public static string Unescape(string value)
        {
            return Regex.Replace(value, "\\\\[\'\"\\\\0abfnrtv]|\\\\u[0-9a-fA-F]{4}|\\\\U[0-9a-fA-F]{8}|\\\\x[0-9a-fA-F]{0,4}", m =>
            {
                if (m.Length == 1) return m.Value;
                if (m.Value[1] == 'u' || m.Value[1] == 'x')
                {
                    return new string((char)Convert.ToInt32(m.Value.Substring(2), 16), 1);
                }
                if (m.Value[1] == 'U')
                {
                    var utf32 = Convert.ToInt32(m.Value.Substring(2), 16);
                    return char.ConvertFromUtf32(utf32);
                }

                switch (m.Value)
                {
                    case @"\'": return "\'";
                    case @"\""": return "\"";
                    case @"\\": return "\\";
                    case @"\0": return "\0";
                    case @"\a": return "\a";
                    case @"\b": return "\b";
                    case @"\f": return "\f";
                    case @"\n": return "\n";
                    case @"\r": return "\r";
                    case @"\t": return "\t";
                    case @"\v": return "\v";
                    default: return m.Value;
                }
            });
        }

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
    }
}
