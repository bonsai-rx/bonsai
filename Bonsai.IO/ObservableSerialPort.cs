using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.IO.Ports;
using System.Reactive.Subjects;

namespace Bonsai.IO
{
    static class ObservableSerialPort
    {
        public static IEnumerable<Action<string>> WriteLine(string portName)
        {
            using (var connection = SerialPortManager.ReserveConnection(portName))
            {
                while (true)
                {
                    yield return value =>
                    {
                        lock (connection)
                        {
                            connection.SerialPort.WriteLine(value);
                        };
                    };
                }
            }
        }

        public static IObservable<string> ReadLine(string portName)
        {
            const string MessageTerminator = "\r\n";

            return Observable.Create<string>(observer =>
            {
                var first = true;
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
                                var messages = data.Split(new[] { MessageTerminator }, StringSplitOptions.None);
                                for (int i = 0; i < messages.Length; i++)
                                {
                                    if (i == messages.Length - 1) data = messages[i];
                                    else if (first) first = false;
                                    else observer.OnNext(messages[i]);
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
