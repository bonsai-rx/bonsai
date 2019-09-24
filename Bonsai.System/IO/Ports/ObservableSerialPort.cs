using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.IO.Ports;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

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
            return Observable.Create<string>(observer =>
            {
                var data = string.Empty;
                var connection = SerialPortManager.ReserveConnection(portName);
                SerialDataReceivedEventHandler dataReceivedHandler;
                var serialPort = connection.SerialPort;
                dataReceivedHandler = (sender, e) =>
                {
                    switch (e.EventType)
                    {
                        case SerialData.Eof: observer.OnCompleted(); break;
                        case SerialData.Chars:
                        default:
                            if (serialPort.IsOpen && serialPort.BytesToRead > 0)
                            {
                                data += serialPort.ReadExisting();
                                var lines = data.Split(new[] { newLine }, StringSplitOptions.None);
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    if (i == lines.Length - 1) data = lines[i];
                                    else observer.OnNext(lines[i]);
                                }
                            }
                            break;
                    }
                };

                connection.SerialPort.DataReceived += dataReceivedHandler;
                return Disposable.Create(() =>
                {
                    connection.SerialPort.DataReceived -= dataReceivedHandler;
                    connection.Dispose();
                });
            });
        }
    }
}
