using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai.IO.Ports;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that creates a connection to an Arduino board
    /// using the Firmata protocol.
    /// </summary>
    [DefaultProperty(nameof(Name))]
    [Description("Creates a connection to an Arduino board using the Firmata protocol.")]
    public class CreateArduino : Source<Arduino>, INamedElement
    {
        readonly ArduinoConfiguration configuration = new ArduinoConfiguration();

        /// <summary>
        /// Gets or sets the optional alias for the Arduino board.
        /// </summary>
        [Description("The optional alias for the Arduino board.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the serial port used to communicate with the Arduino.
        /// </summary>
        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName
        {
            get { return configuration.PortName; }
            set { configuration.PortName = value; }
        }

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The serial baud rate.")]
        public int BaudRate
        {
            get { return configuration.BaudRate; }
            set { configuration.BaudRate = value; }
        }

        /// <summary>
        /// Gets or sets the sampling interval, in milliseconds, between analog and I2C measurements.
        /// </summary>
        [Description("The sampling interval, in milliseconds, between analog and I2C measurements.")]
        public int SamplingInterval
        {
            get { return configuration.SamplingInterval; }
            set { configuration.SamplingInterval = value; }
        }

        /// <summary>
        /// Generates an observable sequence that contains the Firmata connection object.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="Arduino"/> class
        /// representing the Firmata connection.
        /// </returns>
        public override IObservable<Arduino> Generate()
        {
            return Observable.Using(
                () => ArduinoManager.ReserveConnection(Name, configuration),
                resource =>
                {
                    return Observable.Return(resource.Arduino)
                                     .Concat(Observable.Never(resource.Arduino));
                });
        }
    }
}
