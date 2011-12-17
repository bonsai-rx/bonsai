using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bonsai;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(ObjectTextVisualizer), Target = typeof(object))]

namespace Bonsai.Design
{
    public class ObjectTextVisualizer : DialogTypeVisualizer
    {
        Label control;

        public override void Show(object value)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke((Action<object>)Show, value);
            }
            else control.Text = value != null ? value.ToString() : null;
        }

        public override void Load(IServiceProvider provider)
        {
            control = new Label();
            control.AutoSize = true;
            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(control);
            }
        }

        public override void Unload()
        {
            control.Dispose();
            control = null;
        }
    }
}
