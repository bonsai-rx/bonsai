using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Collections.ObjectModel;

namespace Bonsai.Arduino
{
    public class ArduinoConfiguration : SerialPortConfiguration
    {
        readonly SysexConfigurationCollection sysexConfigurationSettings = new SysexConfigurationCollection();

        public ArduinoConfiguration()
        {
            BaudRate = Arduino.DefaultBaudRate;
            SamplingInterval = Arduino.DefaultSamplingInterval;
        }

        [Description("The interval (ms) controlling how often analog and I2C data are sampled and transmitted.")]
        public int SamplingInterval { get; set; }

        [Description("The configuration parameters for communication protocol extensions.")]
        public SysexConfigurationCollection SysexConfigurationSettings
        {
            get { return sysexConfigurationSettings; }
        }
    }
}
