using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO.Ports;

namespace Bonsai.IO
{
    public class SerialPortConfiguration
    {
        public SerialPortConfiguration()
        {
            BaudRate = 9600;
        }

        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The baud rate used by the serial port.")]
        public int BaudRate { get; set; }
    }
}
