using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Collections.ObjectModel;
using System.Windows.Forms.Design;
using System.IO.Ports;
using Bonsai.IO.Design;

namespace Bonsai.Arduino.Design
{
    public partial class ArduinoConfigurationControl : SerialPortConfigurationControl
    {
        protected override object LoadConfiguration()
        {
            return ArduinoManager.LoadConfiguration();
        }

        protected override void SaveConfiguration(object configuration)
        {
            var arduinoConfiguration = configuration as ArduinoConfigurationCollection;
            if (arduinoConfiguration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ArduinoManager.SaveConfiguration(arduinoConfiguration);
        }
    }
}
