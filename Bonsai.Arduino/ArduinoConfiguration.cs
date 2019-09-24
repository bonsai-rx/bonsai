using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Collections.ObjectModel;

namespace Bonsai.Arduino
{
    public class ArduinoConfiguration
    {
        internal static readonly ArduinoConfiguration Default = new ArduinoConfiguration();

        public ArduinoConfiguration()
        {
            BaudRate = Arduino.DefaultBaudRate;
            SamplingInterval = Arduino.DefaultSamplingInterval;
        }

        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The baud rate used by the serial port.")]
        public int BaudRate { get; set; }

        [Description("The interval (ms) controlling how often analog and I2C data are sampled and transmitted.")]
        public int SamplingInterval { get; set; }
    }
}
