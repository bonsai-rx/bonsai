using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Arduino
{
    [DefaultProperty("Pin")]
    [Description("Produces a sequence of digitized analog readings from the specified Arduino input pin.")]
    public class AnalogInput : Source<int>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The analog input pin number from which to take readings.")]
        public int Pin { get; set; }

        public override IObservable<int> Generate()
        {
            return ObservableArduino.AnalogInput(PortName, Pin);
        }
    }
}
