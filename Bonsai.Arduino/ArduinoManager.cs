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

        public static ArduinoDisposable ReserveConnection(string serialPort)
        {
            Tuple<Arduino, RefCountDisposable> arduinoReference;
            if (!openConnections.TryGetValue(serialPort, out arduinoReference))
            {
                Arduino arduino;
                var configuration = LoadConfiguration();
                if (configuration.Contains(serialPort))
                {
                    var arduinoConfiguration = configuration[serialPort];
                    arduino = new Arduino(serialPort, arduinoConfiguration.BaudRate);
                    arduino.Open();

                    arduino.SamplingInterval(arduinoConfiguration.SamplingInterval);
                    foreach (var section in arduinoConfiguration.SysexConfigurationSettings)
                    {
                        section.Configure(arduino);
                    }
                }
                else
                {
                    arduino = new Arduino(serialPort);
                    arduino.Open();
                }

                var dispose = Disposable.Create(() =>
                {
                    arduino.Close();
                    openConnections.Remove(serialPort);
                });

                arduinoReference = Tuple.Create(arduino, new RefCountDisposable(dispose));
                openConnections.Add(serialPort, arduinoReference);
            }

            return new ArduinoDisposable(arduinoReference.Item1, arduinoReference.Item2.GetDisposable());
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
