using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that writes the sequence of digital state transitions
    /// to the specified Arduino output pin.
    /// </summary>
    [DefaultProperty(nameof(Pin))]
    [Description("Writes the sequence of digital state transitions to the specified Arduino output pin.")]
    public class DigitalOutput : Sink<bool>
    {
        /// <summary>
        /// Gets or sets the name of the serial port used to communicate with the Arduino.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the digital output pin number on which to write the state values.
        /// </summary>
        [Description("The digital output pin number on which to write the state values.")]
        public int Pin { get; set; }

        /// <summary>
        /// Writes a sequence of binary states to the specified Arduino digital output pin.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="bool"/> values used to update the state of the specified
        /// Arduino output pin. If a value in the sequence is <see langword="true"/>, the pin
        /// will be set to HIGH; otherwise, the pin will be set to LOW.
        /// </param>
        /// <returns>
        /// A sequence of the <see cref="bool"/> values which have been written into the Arduino
        /// output pin.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the <paramref name="source"/> sequence after
        /// initializing the connection to the Arduino and configuring the digital pin mode
        /// to OUTPUT.
        /// </remarks>
        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return Observable.Using(
                cancellationToken => ArduinoManager.ReserveConnectionAsync(PortName),
                (connection, cancellationToken) =>
                {
                    var pin = Pin;
                    connection.Arduino.PinMode(pin, PinMode.Output);
                    return Task.FromResult(source.Do(value =>
                    {
                        lock (connection.Arduino)
                        {
                            connection.Arduino.DigitalWrite(pin, value);
                        }
                    }));
                });
        }
    }
}
