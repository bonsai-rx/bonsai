using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Drawing.Design;

namespace Bonsai.Arduino
{
    [Description("Writes the sequence of numerical values to the specified Arduino output pin using PWM.")]
    public class AnalogOutput : Sink<int>
    {
        IEnumerable<Action<int>> analogOutput;
        IEnumerator<Action<int>> iterator;

        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string SerialPort { get; set; }

        [Description("The digital output (PWM) pin number for which to write values.")]
        public int Pin { get; set; }

        public override void Process(int input)
        {
            iterator.Current(input);
        }

        public override IDisposable Load()
        {
            analogOutput = ObservableArduino.AnalogOutput(SerialPort, Pin);
            iterator = analogOutput.GetEnumerator();
            iterator.MoveNext();
            return base.Load();
        }

        protected override void Unload()
        {
            iterator.Dispose();
            analogOutput = null;
            base.Unload();
        }
    }
}
