using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reactive.Linq;

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
                () => ArduinoManager.ReserveConnection(PortName),
                connection =>
                {
                    var pin = Pin;
                    connection.Arduino.PinMode(pin, PinMode.Output);
                    return source.Do(value =>
                    {
                        lock (connection.Arduino)
                        {
                            connection.Arduino.DigitalWrite(pin, value);
                        }
                    });
                });
        }
    }
}
