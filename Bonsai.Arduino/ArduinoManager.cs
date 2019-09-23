using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reactive.Disposables;

namespace Bonsai.Arduino
{
    public static class ArduinoManager
    {
        public const string DefaultConfigurationFile = "Arduino.config";
        static readonly Dictionary<string, Tuple<Arduino, RefCountDisposable>> openConnections = new Dictionary<string, Tuple<Arduino, RefCountDisposable>>();
        static readonly object openConnectionsLock = new object();

        public static ArduinoDisposable ReserveConnection(string portName)
        {
            return ReserveConnection(portName, Arduino.DefaultBaudRate, Arduino.DefaultSamplingInterval);
        }

        internal static ArduinoDisposable ReserveConnection(string portName, int baudRate, int samplingInterval)
        {
            if (string.IsNullOrEmpty(portName))
            {
                throw new ArgumentException("A serial port name must be specified.", "portName");
            }

            Tuple<Arduino, RefCountDisposable> connection;
            lock (openConnectionsLock)
            {
                if (!openConnections.TryGetValue(portName, out connection))
                {
                    Arduino arduino;
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(portName))
                    {
                        var arduinoConfiguration = configuration[portName];
                        baudRate = arduinoConfiguration.BaudRate;
                        samplingInterval = arduinoConfiguration.SamplingInterval;
                    }

                    arduino = new Arduino(portName, baudRate);
                    arduino.Open();
                    arduino.SamplingInterval(samplingInterval);
                    var dispose = Disposable.Create(() =>
                    {
                        arduino.Close();
                        openConnections.Remove(portName);
                    });

                    var refCount = new RefCountDisposable(dispose);
                    connection = Tuple.Create(arduino, refCount);
                    openConnections.Add(portName, connection);
                    return new ArduinoDisposable(arduino, refCount);
                }
            }

            return new ArduinoDisposable(connection.Item1, connection.Item2.GetDisposable());
        }

        public static ArduinoConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new ArduinoConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(ArduinoConfigurationCollection));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
            {
                return (ArduinoConfigurationCollection)serializer.Deserialize(reader);
            }
        }

        public static void SaveConfiguration(ArduinoConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(ArduinoConfigurationCollection));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }
    }
}
