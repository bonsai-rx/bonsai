using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Drawing.Design;

namespace Bonsai.Arduino
{
    [Description("Writes the sequence of digital state transitions to the specified Arduino output pin.")]
    public class DigitalOutput : Sink<bool>
    {
        IEnumerable<Action<bool>> digitalOutput;
        IEnumerator<Action<bool>> iterator;

        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string SerialPort { get; set; }

        [Description("The digital output pin number for which to write values.")]
        public int Pin { get; set; }

        public override void Process(bool input)
        {
            iterator.Current(input);
        }

        public override IDisposable Load()
        {
            digitalOutput = ObservableArduino.DigitalOutput(SerialPort, Pin);
            iterator = digitalOutput.GetEnumerator();
            iterator.MoveNext();
            return base.Load();
        }

        protected override void Unload()
        {
            iterator.Dispose();
            digitalOutput = null;
            base.Unload();
        }
    }
}
