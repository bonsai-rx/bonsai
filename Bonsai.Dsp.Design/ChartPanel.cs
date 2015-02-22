using Bonsai.Design;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Dsp.Design
{
    class ChartPanel : ContainerControl, IDialogTypeVisualizerService, IServiceProvider, ITypeVisualizerContext
    {
        InspectBuilder source;
        IServiceProvider provider;

        public ChartPanel(InspectBuilder dataSource, IServiceProvider parentProvider)
        {
            source = dataSource;
            provider = parentProvider;
        }

        public void AddControl(Control control)
        {
            Controls.Add(control);
        }

        public new object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDialogTypeVisualizerService) ||
                serviceType == typeof(ITypeVisualizerContext))
            {
                return this;
            }

            return provider.GetService(serviceType);
        }

        public InspectBuilder Source
        {
            get { return source; }
        }
    }
}
