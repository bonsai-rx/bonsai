using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reactive.Disposables;
using System.IO.Ports;

namespace Bonsai.IO
{
    public static class SerialPortManager
    {
        public const string DefaultConfigurationFile = "SerialPort.config";
        static readonly Dictionary<string, Tuple<SerialPort, RefCountDisposable>> openConnections = new Dictionary<string, Tuple<SerialPort, RefCountDisposable>>();
        static readonly object openConnectionsLock = new object();

        public static SerialPortDisposable ReserveConnection(string portName)
        {
            return ReserveConnection(portName, SerialPortConfiguration.Default);
        }

        internal static SerialPortDisposable ReserveConnection(string portName, SerialPortConfiguration serialPortConfiguration)
        {
            if (string.IsNullOrEmpty(portName))
            {
                if (!string.IsNullOrEmpty(serialPortConfiguration.PortName)) portName = serialPortConfiguration.PortName;
                else throw new ArgumentException("An alias or serial port name must be specified.", "portName");
            }

            Tuple<SerialPort, RefCountDisposable> connection;
            lock (openConnectionsLock)
            {
                if (!openConnections.TryGetValue(portName, out connection))
                {
                    var serialPortName = serialPortConfiguration.PortName;
                    if (string.IsNullOrEmpty(serialPortName)) serialPortName = portName;

                    var configuration = LoadConfiguration();
                    if (configuration.Contains(serialPortName))
                    {
                        serialPortConfiguration = configuration[serialPortName];
                    }

                    var serialPort = new SerialPort(
                        serialPortName,
                        serialPortConfiguration.BaudRate,
                        serialPortConfiguration.Parity,
                        serialPortConfiguration.DataBits,
                        serialPortConfiguration.StopBits);
                    serialPort.ReceivedBytesThreshold = serialPortConfiguration.ReceivedBytesThreshold;
                    serialPort.ReadBufferSize = serialPortConfiguration.ReadBufferSize;
                    serialPort.WriteBufferSize = serialPortConfiguration.WriteBufferSize;
                    serialPort.ParityReplace = serialPortConfiguration.ParityReplace;
                    serialPort.Handshake = serialPortConfiguration.Handshake;
                    serialPort.DiscardNull = serialPortConfiguration.DiscardNull;
                    serialPort.DtrEnable = serialPortConfiguration.DtrEnable;
                    serialPort.RtsEnable = serialPortConfiguration.RtsEnable;

                    var encoding = serialPortConfiguration.Encoding;
                    if (!string.IsNullOrEmpty(encoding))
                    {
                        serialPort.Encoding = Encoding.GetEncoding(encoding);
                    }

                    serialPort.Open();
                    serialPort.ReadExisting();
                    var dispose = Disposable.Create(() =>
                    {
                        serialPort.Close();
                        openConnections.Remove(portName);
                    });

                    var refCount = new RefCountDisposable(dispose);
                    connection = Tuple.Create(serialPort, refCount);
                    openConnections.Add(portName, connection);
                    return new SerialPortDisposable(serialPort, refCount);
                }
            }

            return new SerialPortDisposable(connection.Item1, connection.Item2.GetDisposable());
        }

        public static SerialPortConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new SerialPortConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(SerialPortConfigurationCollection));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
            {
                return (SerialPortConfigurationCollection)serializer.Deserialize(reader);
            }
        }

        public static void SaveConfiguration(SerialPortConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(SerialPortConfigurationCollection));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }
    }
}
