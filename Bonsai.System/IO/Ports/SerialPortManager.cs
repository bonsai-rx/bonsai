using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reactive.Disposables;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace Bonsai.IO.Ports
{
    internal static class SerialPortManager
    {
        public const string DefaultNewLine = @"\r\n";
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

            lock (SyncRoot)
            {
                if (!openConnections.TryGetValue(portName, out var connection))
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

                    var serialPort = new SerialPort(
                        serialPortName,
                        serialPortConfiguration.BaudRate,
                        serialPortConfiguration.Parity,
                        serialPortConfiguration.DataBits,
                        serialPortConfiguration.StopBits);
                    if (!IsRunningOnMono)
                    {
                        serialPort.ReceivedBytesThreshold = serialPortConfiguration.ReceivedBytesThreshold;
                        serialPort.ParityReplace = serialPortConfiguration.ParityReplace;
                        serialPort.DiscardNull = serialPortConfiguration.DiscardNull;
                    }
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

                    var newLine = serialPortConfiguration.NewLine;
                    if (!string.IsNullOrEmpty(newLine))
                    {
                        serialPort.NewLine = Unescape(newLine);
                    }

                    serialPort.Open();

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
