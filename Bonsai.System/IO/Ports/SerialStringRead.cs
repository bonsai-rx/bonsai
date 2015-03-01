using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.IO
{
    [Description("Sources individual lines of text data from a serial port.")]
    public class SerialStringRead : Source<string>
    {
        [Description("The name of the serial port.")]
        [Editor("Bonsai.IO.Design.SerialPortConfigurationEditor, Bonsai.System.Design", typeof(UITypeEditor))]
        public string PortName { get; set; }

        public override IObservable<string> Generate()
        {
            return ObservableSerialPort.ReadLine(PortName);
        }
    }
}
