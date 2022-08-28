using System;
using System.Collections.Generic;
using System.IO.Ports;
using Bonsai.Design;
using System.Drawing.Design;

namespace Bonsai.IO.Design
{
    /// <summary>
    /// Provides a base class for serial port drop-down editor controls
    /// with custom configuration dialogs.
    /// </summary>
    [Obsolete]
    public partial class SerialPortConfigurationControl : ConfigurationDropDown
    {
        /// <inheritdoc/>
        protected override IEnumerable<string> GetConfigurationNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <inheritdoc/>
        protected override object LoadConfiguration()
        {
            return null;
        }

        /// <inheritdoc/>
        protected override void SaveConfiguration(object configuration)
        {
        }

        /// <inheritdoc/>
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
