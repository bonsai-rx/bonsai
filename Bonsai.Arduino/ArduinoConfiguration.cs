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
        readonly SysexConfigurationCollection sysexConfigurationSettings = new SysexConfigurationCollection();

        public ArduinoConfiguration()
        {
            BaudRate = Arduino.DefaultBaudRate;
            SamplingInterval = Arduino.DefaultSamplingInterval;
        }

        [TypeConverter(typeof(SerialPortNameConverter))]
        public string PortName { get; set; }

        [TypeConverter(typeof(BaudRateConverter))]
        public int BaudRate { get; set; }

        public int SamplingInterval { get; set; }

        public SysexConfigurationCollection SysexConfigurationSettings
        {
            get { return sysexConfigurationSettings; }
        }
    }
}
