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
        static readonly Dictionary<string, SerialPortReference> openConnections = new Dictionary<string, SerialPortReference>();
        class SerialPortReference : IDisposable
        {
            string output;
            const char MessageTerminator = '\n';

            public SerialPortReference(SerialPort serialPort)
            {
                SerialPort = serialPort;
                MessageReceived = new Subject<string>();
                SerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
                SerialPort.Open();
                output = string.Empty;
            }

            void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                switch (e.EventType)
                {
                    case SerialData.Eof: MessageReceived.OnCompleted(); break;
                    case SerialData.Chars:
                        output += SerialPort.ReadExisting();
                        var messages = output.Split(MessageTerminator);
                        for (int i = 0; i < messages.Length - 1; i++)
                        {
                            MessageReceived.OnNext(messages[i]);
                        }
                        break;
                }
            }

            public SerialPort SerialPort { get; private set; }

            public Subject<string> MessageReceived { get; private set; }

            public int RefCount { get; set; }

            public void Dispose()
            {
                SerialPort.Close();
                MessageReceived.Dispose();
            }
        }

        static SerialPortReference ReserveConnection(string portName)
        {
            SerialPortReference serialReference;
            if (!openConnections.TryGetValue(portName, out serialReference))
            {
                var arduino = new SerialPort(portName, 115200);
                serialReference = new SerialPortReference(arduino);
                openConnections.Add(portName, serialReference);
            }

            serialReference.RefCount++;
            return serialReference;
        }

        static void ReleaseConnection(string serialPort)
        {
            var arduinoReference = openConnections[serialPort];
            if (--arduinoReference.RefCount <= 0)
            {
                arduinoReference.Dispose();
                openConnections.Remove(serialPort);
            }
        }

        public static IEnumerable<Action<string>> WriteLine(string portName)
        {
            var connection = ReserveConnection(portName);

            try
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
            finally { ReleaseConnection(portName); }
        }

        public static IObservable<string> ReadLine(string portName)
        {
            var connection = ReserveConnection(portName);
            return connection.MessageReceived;
        }
    }
}
