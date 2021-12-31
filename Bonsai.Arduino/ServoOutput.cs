using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that writes a sequence of angular positions to control
    /// a servomotor connected to an Arduino output pin.
    /// </summary>
    [DefaultProperty(nameof(Pin))]
    [Description("Writes a sequence of angular positions to control a servomotor connected to an Arduino output pin.")]
    public class ServoOutput : Sink<int>
    {
        /// <summary>
        /// Gets or sets the name of the serial port used to communicate with the Arduino.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the digital output pin number to which the servo is connected.
        /// </summary>
        [Description("The digital output pin number to which the servo is connected.")]
        public int Pin { get; set; }

        /// <summary>
        /// Writes a sequence of angular position values to control a servomotor connected to the
        /// specified Arduino output pin.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="int"/> values specifying angular positions, in degrees from
        /// 0 to 180, used to control the servomotor connected to the specified Arduino output pin.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="int"/> values containing the angular positions which have been
        /// used to control the servomotor connected to the specified Arduino output pin.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the source sequence after initializing the connection
        /// to the Arduino and configuring the digital pin as a Servo output.
        /// </remarks>
        public override IObservable<int> Process(IObservable<int> source)
        {
            return Observable.Using(
                cancellationToken => ArduinoManager.ReserveConnectionAsync(PortName),
                (connection, cancellationToken) =>
                {
                    var pin = Pin;
                    connection.Arduino.PinMode(pin, PinMode.Servo);
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
