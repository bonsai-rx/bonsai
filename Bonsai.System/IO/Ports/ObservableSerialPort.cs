using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reactive.Concurrency;

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
                Action disposeAction = default;
                var connection = SerialPortManager.ReserveConnection(portName);
                SerialDataReceivedEventHandler dataReceivedHandler;
                var serialPort = connection.SerialPort;
                var baseStream = connection.SerialPort.BaseStream;
                dataReceivedHandler = (sender, e) =>
                {
                    try
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
                    }
                    finally
                    {
                        // We need a volatile read here to prevent reordering of
                        // instructions on access to the shared dispose delegate
                        var dispose = Volatile.Read(ref disposeAction);
                        if (dispose != null)
                        {
                            // If we reach this branch, we might be in deadlock
                            // so we share the responsibility of disposing the
                            // serial port.
                            dispose();
                            Volatile.Write(ref disposeAction, null);
                        }
                    }
                };
                connection.SerialPort.DataReceived += dataReceivedHandler;
                return Disposable.Create(() =>
                {
                    connection.SerialPort.DataReceived -= dataReceivedHandler;

                    // Arm the dispose call. We do not need a memory barrier here
                    // since both threads are sharing full mutexes and stores
                    // will be eventually updated (we don't care exactly when)
                    disposeAction = connection.Dispose;

                    // We do an async spin lock until someone can dispose the serial port.
                    // Since the dispose call is idempotent it is enough to guarantee
                    // at-least-once semantics
                    void TryDispose()
                    {
                        // Same as above we need a volatile read here to prevent
                        // reordering of instructions
                        var dispose = Volatile.Read(ref disposeAction);
                        if (dispose == null) return;

                        // The SerialPort class holds a lock on base stream to
                        // ensure synchronization between calls to Dispose and
                        // calls to DataReceived handler
                        if (Monitor.TryEnter(baseStream))
                        {
                            // If we enter the critical section we can go ahead and
                            // dispose the serial port
                            try
                            {
                                dispose();
                                Volatile.Write(ref disposeAction, null);
                            }
                            finally { Monitor.Exit(baseStream); }
                        }
                        else
                        {
                            // If we reach this branch we may be in deadlock so we
                            // need to release this thread
                            DefaultScheduler.Instance.Schedule(TryDispose);
                        }
                    }

                    // Run the spin lock
                    TryDispose();
                });
            });
        }
    }
}
