using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reactive.Linq;

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
                () => ArduinoManager.ReserveConnection(PortName),
                connection =>
                {
                    var pin = Pin;
                    connection.Arduino.PinMode(pin, PinMode.Pwm);
                    return source.Do(value =>
                    {
                        lock (connection.Arduino)
                        {
                            connection.Arduino.AnalogWrite(pin, value);
                        }
                    });
                });
        }
    }
}
