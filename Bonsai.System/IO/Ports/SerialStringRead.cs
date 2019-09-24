using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;

namespace Bonsai.IO
{
    [DefaultProperty("PortName")]
    [Description("Sources individual lines of text data from a serial port.")]
    public class SerialStringRead : Source<string>
    {
        public SerialStringRead()
        {
            NewLine = ObservableSerialPort.DefaultNewLine;
        }

        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        [Description("The value used to interpret lines sourced from the serial port.")]
        public string NewLine { get; set; }

        public override IObservable<string> Generate()
        {
            var newLine = ObservableSerialPort.Unescape(NewLine);
            return ObservableSerialPort.ReadLine(PortName, newLine);
        }
    }
}
