using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    [DefaultProperty("Pin")]
    [Description("Uses a sequence of numerical values from 0 to 180 to control a servomotor connected to an Arduino output pin.")]
    public class ServoOutput : Sink<int>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital output pin number to which the servo is connected.")]
        public int Pin { get; set; }

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
