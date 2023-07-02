using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Threading;

namespace Bonsai.Arduino
{
    internal static class ArduinoManager
    {
        public const string DefaultConfigurationFile = "Arduino.config";
        static readonly Dictionary<string, Tuple<Arduino, RefCountDisposable>> openConnections = new();
        internal static readonly object SyncRoot = new();

        public static ArduinoDisposable ReserveConnection(string portName)
        {
            return ReserveConnection(portName, ArduinoConfiguration.Default);
        }

        public static async Task<ArduinoDisposable> ReserveConnectionAsync(string portName)
        {
            return await Task.Run(() => ReserveConnection(portName, ArduinoConfiguration.Default));
        }

        internal static ArduinoDisposable ReserveConnection(string portName, ArduinoConfiguration arduinoConfiguration)
        {
            Tuple<Arduino, RefCountDisposable> connection = default;
            lock (SyncRoot)
            {
                if (string.IsNullOrEmpty(portName))
                {
                    if (!string.IsNullOrEmpty(arduinoConfiguration.PortName)) portName = arduinoConfiguration.PortName;
                    else if (openConnections.Count == 1) connection = openConnections.Values.Single();
                    else throw new ArgumentException("An alias or serial port name must be specified.", nameof(portName));
                }

                if (connection == null && !openConnections.TryGetValue(portName, out connection))
                {
                    var serialPortName = arduinoConfiguration.PortName;
                    if (string.IsNullOrEmpty(serialPortName)) serialPortName = portName;

#pragma warning disable CS0612 // Type or member is obsolete
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(serialPortName))
                    {
                        arduinoConfiguration = configuration[serialPortName];
                    }
#pragma warning restore CS0612 // Type or member is obsolete

                    var cancellation = new CancellationTokenSource();
                    var arduino = new Arduino(serialPortName, arduinoConfiguration.BaudRate);
                    arduino.Open(cancellation.Token);
                    arduino.SamplingInterval(arduinoConfiguration.SamplingInterval);
                    var dispose = Disposable.Create(() =>
                    {
                        cancellation.Cancel();
                        openConnections.Remove(portName);
                    });

                    var refCount = new RefCountDisposable(dispose);
                    connection = Tuple.Create(arduino, refCount);
                    openConnections.Add(portName, connection);
                    return new ArduinoDisposable(arduino, refCount);
                }

                return new ArduinoDisposable(connection.Item1, connection.Item2.GetDisposable());
            }
        }

        [Obsolete]
        public static ArduinoConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new ArduinoConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(ArduinoConfigurationCollection));
            using var reader = XmlReader.Create(DefaultConfigurationFile);
            return (ArduinoConfigurationCollection)serializer.Deserialize(reader);
        }
    }
}
