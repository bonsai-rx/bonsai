using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(CvSeqVisualizer), Target = typeof(CvSeq))]

namespace Bonsai.Vision.Design
{
    public class CvSeqVisualizer : DialogTypeVisualizer
    {
        IplImage output;
        IplImageControl control;

        public override void Show(object value)
        {
            output.SetZero();

            var contours = (CvSeq)value;
            if (!contours.IsInvalid)
            {
                Core.cvDrawContours(output, contours, CvScalar.All(255), CvScalar.All(0), 1, -1, 8, CvPoint.Zero);
            }

            control.Image = output;
        }

        public override void Load(IServiceProvider provider)
        {
            var size = (CvSize)provider.GetService(typeof(CvSize));
            output = new IplImage(size, 8, 1);
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
