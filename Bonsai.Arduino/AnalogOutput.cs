using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that writes the sequence of numerical values to the
    /// specified Arduino output pin using PWM.
    /// </summary>
    [DefaultProperty(nameof(Pin))]
    [Description("Writes the sequence of numerical values to the specified Arduino output pin using PWM.")]
    public class AnalogOutput : Sink<int>
    {
        /// <summary>
        /// Gets or sets the name of the serial port used to communicate with the Arduino.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the digital output (PWM) pin number on which to write values.
        /// </summary>
        [Description("The digital output (PWM) pin number on which to write values.")]
        public int Pin { get; set; }

        /// <summary>
        /// Writes a sequence of <see cref="int"/> values to the specified Arduino output pin using PWM.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="int"/> values to write into the specified Arduino output pin.
        /// </param>
        /// <returns>
        /// A sequence of the <see cref="int"/> values which have been written into the Arduino
        /// output pin.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the <paramref name="source"/> sequence after initializing
        /// the connection to the Arduino and configuring the output pin mode to PWM.
        /// </remarks>
        public override IObservable<int> Process(IObservable<int> source)
        {
            return Observable.Using(
                cancellationToken => ArduinoManager.ReserveConnectionAsync(PortName),
                (connection, cancellationToken) =>
                {
                    var pin = Pin;
                    connection.Arduino.PinMode(pin, PinMode.Pwm);
                    return Task.FromResult(source.Do(value =>
                    {
                        lock (connection.Arduino)
                        {
                            connection.Arduino.AnalogWrite(pin, value);
                        }
                    }));
                });
        }
    }
}
