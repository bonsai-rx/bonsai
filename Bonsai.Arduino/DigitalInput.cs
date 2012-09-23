using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Drawing.Design;

namespace Bonsai.Arduino
{
    [Description("Produces a sequence of digital state transitions from the specified Arduino input pin.")]
    public class DigitalInput : Source<bool>
    {
        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string SerialPort { get; set; }

        [Description("The digital input pin number from which to take readings.")]
        public int Pin { get; set; }

        protected override IObservable<bool> Generate()
        {
            return ObservableArduino.DigitalInput(SerialPort, Pin);
        }
    }
}
