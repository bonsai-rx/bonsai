using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reactive.Disposables;
using System.IO.Ports;

namespace Bonsai.IO
{
    internal static class SerialPortManager
    {
        public const string DefaultConfigurationFile = "SerialPort.config";
        static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
        static readonly Dictionary<string, Tuple<SerialPort, RefCountDisposable>> openConnections = new();
        internal static readonly object SyncRoot = new();

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
            lock (SyncRoot)
            {
                if (!openConnections.TryGetValue(portName, out connection))
                {
                    var serialPortName = serialPortConfiguration.PortName;
                    if (string.IsNullOrEmpty(serialPortName)) serialPortName = portName;

#pragma warning disable CS0612 // Type or member is obsolete
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(serialPortName))
                    {
                        serialPortConfiguration = configuration[serialPortName];
                    }
#pragma warning restore CS0612 // Type or member is obsolete

                    SerialPort serialPort;
                    if (IsRunningOnMono)
                    {
                        var pollingPort = new PollingSerialPort(
                            serialPortName,
                            serialPortConfiguration.BaudRate,
                            serialPortConfiguration.Parity,
                            serialPortConfiguration.DataBits,
                            serialPortConfiguration.StopBits);
                        serialPort = pollingPort;
                        ConfigureSerialPort(serialPort);
                        pollingPort.Open();
                    }
                    else
                    {
                        serialPort = new SerialPort(
                            serialPortName,
                            serialPortConfiguration.BaudRate,
                            serialPortConfiguration.Parity,
                            serialPortConfiguration.DataBits,
                            serialPortConfiguration.StopBits);
                        serialPort.ReceivedBytesThreshold = serialPortConfiguration.ReceivedBytesThreshold;
                        serialPort.ParityReplace = serialPortConfiguration.ParityReplace;
                        serialPort.DiscardNull = serialPortConfiguration.DiscardNull;
                        ConfigureSerialPort(serialPort);
                        serialPort.Open();
                    }

                    void ConfigureSerialPort(SerialPort serialPort)
                    {
                        serialPort.ReadBufferSize = serialPortConfiguration.ReadBufferSize;
                        serialPort.WriteBufferSize = serialPortConfiguration.WriteBufferSize;
                        serialPort.Handshake = serialPortConfiguration.Handshake;
                        serialPort.DtrEnable = serialPortConfiguration.DtrEnable;
                        serialPort.RtsEnable = serialPortConfiguration.RtsEnable;

                        var encoding = serialPortConfiguration.Encoding;
                        if (!string.IsNullOrEmpty(encoding))
                        {
                            serialPort.Encoding = Encoding.GetEncoding(encoding);
                        }
                    }

                    if (serialPort.BytesToRead > 0)
                    {
                        serialPort.ReadExisting();
                    }
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

                return new SerialPortDisposable(connection.Item1, connection.Item2.GetDisposable());
            }
        }

        [Obsolete]
        public static SerialPortConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new SerialPortConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(SerialPortConfigurationCollection));
            using var reader = XmlReader.Create(DefaultConfigurationFile);
            return (SerialPortConfigurationCollection)serializer.Deserialize(reader);
        }

        [Obsolete]
        public static void SaveConfiguration(SerialPortConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(SerialPortConfigurationCollection));
            using var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true });
            serializer.Serialize(writer, configuration);
        }
    }
}
