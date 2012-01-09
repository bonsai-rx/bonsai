using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using Bonsai.Vision;

[assembly: TypeVisualizer(typeof(ContoursVisualizer), Target = typeof(Contours))]

namespace Bonsai.Vision.Design
{
    public class ContoursVisualizer : DialogTypeVisualizer
    {
        IplImageControl control;

        public override void Show(object value)
        {
            var contours = (Contours)value;
            var output = new IplImage(contours.ImageSize, 8, 1);
            output.SetZero();

            if (!contours.FirstContour.IsInvalid)
            {
                Core.cvDrawContours(output, contours.FirstContour, CvScalar.All(255), CvScalar.All(0), 1, -1, 8, CvPoint.Zero);
            }

            control.Image = output;
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
