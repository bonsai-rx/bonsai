using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that generates a sequence of system exclusive messages
    /// received from the specified Arduino.
    /// </summary>
    [DefaultProperty(nameof(Feature))]
    [Description("Generates a sequence of system exclusive messages received from the specified Arduino.")]
    public class ReceiveSysex : Source<byte[]>
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
        /// Generates an observable sequence of all the system exclusive messages with the
        /// specified feature ID received from the Arduino.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="byte"/> arrays containing the payload data which was
        /// included with each system exclusive message received from the Arduino.
        /// </returns>
        public override IObservable<byte[]> Generate()
        {
            return Observable.Create<byte[]>(async observer =>
            {
                var connection = await ArduinoManager.ReserveConnectionAsync(PortName);
                EventHandler<SysexReceivedEventArgs> sysexReceived;
                sysexReceived = (sender, e) =>
                {
                    if (e.Feature == Feature)
                    {
                        observer.OnNext(e.Args);
                    }
                };

                connection.Arduino.SysexReceived += sysexReceived;
                return Disposable.Create(() =>
                {
                    connection.Arduino.SysexReceived -= sysexReceived;
                    connection.Dispose();
                });
            });
        }
    }
}
