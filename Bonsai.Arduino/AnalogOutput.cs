using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    [DefaultProperty("Pin")]
    [Description("Writes the sequence of numerical values to the specified Arduino output pin using PWM.")]
    public class AnalogOutput : Sink<int>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital output (PWM) pin number for which to write values.")]
        public int Pin { get; set; }

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
