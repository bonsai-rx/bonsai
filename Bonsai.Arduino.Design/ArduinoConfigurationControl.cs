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
using Bonsai.Design;
using System.Drawing.Design;

namespace Bonsai.Arduino.Design
{
    public partial class ArduinoConfigurationControl : ConfigurationDropDown
    {
        protected override IEnumerable<string> GetConfigurationNames()
        {
            return SerialPort.GetPortNames();
        }

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

        protected override UITypeEditor CreateConfigurationEditor(Type type)
        {
            return new ArduinoConfigurationCollectionEditor(type);
        }

        class ArduinoConfigurationCollectionEditor : DescriptiveCollectionEditor
        {
            public ArduinoConfigurationCollectionEditor(Type type)
                : base(type)
            {
            }

            protected override string GetDisplayText(object value)
            {
                var configuration = value as ArduinoConfiguration;
                if (configuration != null)
                {
                    if (!string.IsNullOrEmpty(configuration.PortName))
                    {
                        return configuration.PortName;
                    }

                    return typeof(ArduinoConfiguration).Name;
                }

                return base.GetDisplayText(value);
            }
        }
    }
}
