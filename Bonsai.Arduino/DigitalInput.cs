using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Arduino
{
    [DefaultProperty("Pin")]
    [Description("Produces a sequence of digital state transitions from the specified Arduino input pin.")]
    public class DigitalInput : Source<bool>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital input pin number from which to take readings.")]
        public int Pin { get; set; }

        public override IObservable<bool> Generate()
        {
            return ObservableArduino.DigitalInput(PortName, Pin);
        }
    }
}
