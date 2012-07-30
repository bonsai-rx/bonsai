using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;

namespace Bonsai.IO
{
    public class SerialStringRead : Source<string>
    {
        [TypeConverter(typeof(SerialPortNameConverter))]
        public string SerialPort { get; set; }

        protected override IObservable<string> Generate()
        {
            return ObservableSerialPort.ReadLine(SerialPort);
        }
    }
}
