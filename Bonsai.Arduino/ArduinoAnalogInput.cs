using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Drawing.Design;

namespace Bonsai.Arduino
{
    public class ArduinoAnalogInput : Source<int>
    {
        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        public string SerialPort { get; set; }

        public int Pin { get; set; }

        protected override IObservable<int> Generate()
        {
            return ObservableArduino.AnalogInput(SerialPort, Pin);
        }
    }
}
