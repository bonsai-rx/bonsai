using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    [DefaultProperty("Feature")]
    [Description("Sends a sequence of system exclusive messages to the specified Arduino.")]
    public class SendSysex : Sink<byte[]>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The feature ID used to identify the system exclusive message payload.")]
        public int Feature { get; set; }

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
