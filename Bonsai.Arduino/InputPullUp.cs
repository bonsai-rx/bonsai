using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    [DefaultProperty("Pin")]
    [Description("Produces a sequence of digital state transitions from the specified Arduino input pin in pull-up mode.")]
    public class InputPullUp : Source<bool>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital input pull-up pin number from which to take readings.")]
        public int Pin { get; set; }

        public override IObservable<bool> Generate()
        {
            return ObservableArduino.DigitalInput(PortName, Pin, PinMode.InputPullUp);
        }
    }
}
