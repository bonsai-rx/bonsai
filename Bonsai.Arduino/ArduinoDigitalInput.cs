using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;

namespace Bonsai.Arduino
{
    public class ArduinoDigitalInput : Source<bool>
    {
        [TypeConverter(typeof(SerialPortNameConverter))]
        public string SerialPort { get; set; }

        public int Pin { get; set; }

        protected override IObservable<bool> Generate()
        {
            return ObservableArduino.DigitalInput(SerialPort, Pin);
        }
    }
}
