using System.ComponentModel;
using Bonsai.IO.Ports;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents configuration settings used to initialize a Firmata serial connection.
    /// </summary>
    public class ArduinoConfiguration
    {
        internal static readonly ArduinoConfiguration Default = new ArduinoConfiguration();

        /// <summary>
        /// Initializes a new instance of the <see cref="ArduinoConfiguration"/> class.
        /// </summary>
        public ArduinoConfiguration()
        {
            BaudRate = Arduino.DefaultBaudRate;
            SamplingInterval = Arduino.DefaultSamplingInterval;
        }

        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The serial baud rate.")]
        public int BaudRate { get; set; }

        /// <summary>
        /// Gets or sets the sampling interval, in milliseconds, between analog and I2C measurements.
        /// </summary>
        [Description("The sampling interval, in milliseconds, between analog and I2C measurements.")]
        public int SamplingInterval { get; set; }
    }
}
