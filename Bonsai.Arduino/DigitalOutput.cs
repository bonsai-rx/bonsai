using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    [DefaultProperty("Pin")]
    [Description("Writes the sequence of digital state transitions to the specified Arduino output pin.")]
    public class DigitalOutput : Sink<bool>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital output pin number for which to write values.")]
        public int Pin { get; set; }

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
