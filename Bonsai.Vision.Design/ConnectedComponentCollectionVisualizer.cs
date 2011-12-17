using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using Bonsai.Vision;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionVisualizer), Target = typeof(ConnectedComponentCollection))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentCollectionVisualizer : DialogTypeVisualizer
    {
        IplImage output;
        IplImageControl control;

        public override void Show(object value)
        {
            output.SetZero();

            var components = (ConnectedComponentCollection)value;
            foreach (var component in components)
            {
                var center = component.Center;
                var angle = component.Angle;
                var point1 = new CvPoint((int)(center.X + 10 * Math.Cos(angle)), (int)(center.Y + 10 * Math.Sin(angle)));
                var point2 = new CvPoint((int)(center.X - 10 * Math.Cos(angle)), (int)(center.Y - 10 * Math.Sin(angle)));

                Core.cvDrawContours(output, component.Contour, CvScalar.All(255), CvScalar.All(0), 0, -1, 8, CvPoint.Zero);
                Core.cvDrawContours(output, component.Contour, CvScalar.Rgb(255, 0, 0), CvScalar.Rgb(0, 0, 255), 0, 1, 8, CvPoint.Zero);
                Core.cvLine(output, point1, point2, CvScalar.Rgb(0, 0, 255), 1, 8, 0);
                Core.cvCircle(output, center, 2, CvScalar.Rgb(255, 0, 0), -1, 8, 0);
            }

            control.Image = output;
        }

        public override void Load(IServiceProvider provider)
        {
            var size = (CvSize)provider.GetService(typeof(CvSize));
            output = new IplImage(size, 8, 3);
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
