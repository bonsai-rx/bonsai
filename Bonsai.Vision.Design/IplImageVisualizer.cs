using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(IplImageVisualizer), Target = typeof(IplImage))]

namespace Bonsai.Vision.Design
{
    public class IplImageVisualizer : DialogTypeVisualizer
    {
        IplImageControl control;

        public override void Show(object value)
        {
            var image = (IplImage)value;
            control.Image = image;
        }

        public override void Load(IServiceProvider provider)
        {
            control = new IplImageControl();
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
