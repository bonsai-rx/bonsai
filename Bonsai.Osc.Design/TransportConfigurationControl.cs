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
using Bonsai.Osc.Net;

namespace Bonsai.Osc.Design
{
    public partial class TransportConfigurationControl : ConfigurationControl
    {
        protected override IEnumerable<string> GetConfigurationNames()
        {
            return TransportManager.LoadConfiguration().Select(configuration => configuration.Name);
        }

        protected override object LoadConfiguration()
        {
            return TransportManager.LoadConfiguration();
        }

        protected override void SaveConfiguration(object configuration)
        {
            var serialPortConfiguration = configuration as TransportConfigurationCollection;
            if (serialPortConfiguration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            TransportManager.SaveConfiguration(serialPortConfiguration);
        }

        protected override CollectionEditor CreateConfigurationEditor(Type type)
        {
            return new TransportConfigurationCollectionEditor(type);
        }

        class TransportConfigurationCollectionEditor : DescriptiveCollectionEditor
        {
            public TransportConfigurationCollectionEditor(Type type)
                : base(type)
            {
            }

            protected override Type[] CreateNewItemTypes()
            {
                return new[]
                {
                    typeof(UdpConfiguration),
                    typeof(TcpServerConfiguration),
                    typeof(TcpClientConfiguration)
                };
            }
        }
    }
}
