using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace Bonsai.Arduino
{
    public static class ArduinoManager
    {
        public const string DefaultConfigurationFile = "Arduino.config";
        static readonly Dictionary<string, ArduinoReference> openConnections = new Dictionary<string, ArduinoReference>();
        class ArduinoReference : IDisposable
        {
            public ArduinoReference(Arduino arduino)
            {
                Arduino = arduino;
                if (!Arduino.IsOpen)
                {
                    Arduino.Open();
                }
            }

            public Arduino Arduino { get; private set; }

            public int RefCount { get; set; }

            public void Dispose()
            {
                Arduino.Close();
            }
        }

        public static Arduino ReserveConnection(string serialPort)
        {
            ArduinoReference arduinoReference;
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
                else arduino = new Arduino(serialPort);

                arduinoReference = new ArduinoReference(arduino);
                openConnections.Add(serialPort, arduinoReference);
            }

            arduinoReference.RefCount++;
            return arduinoReference.Arduino;
        }

        public static void ReleaseConnection(string serialPort)
        {
            var arduinoReference = openConnections[serialPort];
            if (--arduinoReference.RefCount <= 0)
            {
                arduinoReference.Dispose();
                openConnections.Remove(serialPort);
            }
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
