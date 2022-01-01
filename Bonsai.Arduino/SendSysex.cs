using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that sends a sequence of system exclusive messages to the specified Arduino.
    /// </summary>
    [DefaultProperty(nameof(Feature))]
    [Description("Sends a sequence of system exclusive messages to the specified Arduino.")]
    public class SendSysex : Sink<byte[]>
    {
        /// <summary>
        /// Gets or sets the name of the serial port used to communicate with the Arduino.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the feature ID used to identify the system exclusive message payload.
        /// </summary>
        [Description("The feature ID used to identify the system exclusive message payload.")]
        public int Feature { get; set; }

        /// <summary>
        /// Writes a sequence of system exclusive messages to the specified Arduino.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="byte"/> arrays specifying the payload data to include in each
        /// of the system exclusive messages sent to the Arduino. The specified feature ID will
        /// be used to identify each message.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="byte"/> arrays containing the payload data which was
        /// included with each system exclusive message sent to the Arduino.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the source sequence after initializing the connection
        /// to the Arduino.
        /// </remarks>
        public override IObservable<byte[]> Process(IObservable<byte[]> source)
        {
            return Observable.Using(
                cancellationToken => ArduinoManager.ReserveConnectionAsync(PortName),
                (connection, cancellationToken) => Task.FromResult(source.Do(value =>
                {
                    lock (connection.Arduino)
                    {
                        connection.Arduino.SendSysex((byte)Feature, value);
                    }
                })));
        }
    }
}
