using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.IO
{
    public class SerialStringRead : Source<string>
    {
        [Editor("Bonsai.IO.Design.SerialPortConfigurationEditor, Bonsai.System.Design", typeof(UITypeEditor))]
        public string PortName { get; set; }

        public override IObservable<string> Generate()
        {
            return ObservableSerialPort.ReadLine(PortName);
        }
    }
}
