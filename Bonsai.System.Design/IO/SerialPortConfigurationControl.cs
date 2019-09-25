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

namespace Bonsai.IO.Design
{
    [Obsolete]
    public partial class SerialPortConfigurationControl : ConfigurationDropDown
    {
        protected override IEnumerable<string> GetConfigurationNames()
        {
            return SerialPort.GetPortNames();
        }

        protected override object LoadConfiguration()
        {
            return SerialPortManager.LoadConfiguration();
        }

        protected override void SaveConfiguration(object configuration)
        {
            var serialPortConfiguration = configuration as SerialPortConfigurationCollection;
            if (serialPortConfiguration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            SerialPortManager.SaveConfiguration(serialPortConfiguration);
        }

        protected override UITypeEditor CreateConfigurationEditor(Type type)
        {
            return new SerialPortConfigurationCollectionEditor(type);
        }

        class SerialPortConfigurationCollectionEditor : DescriptiveCollectionEditor
        {
            public SerialPortConfigurationCollectionEditor(Type type)
                : base(type)
            {
            }

            protected override string GetDisplayText(object value)
            {
                var configuration = value as SerialPortConfiguration;
                if (configuration != null)
                {
                    if (!string.IsNullOrEmpty(configuration.PortName))
                    {
                        return configuration.PortName;
                    }

                    return typeof(SerialPortConfiguration).Name;
                }

                return base.GetDisplayText(value);
            }
        }
    }
}
