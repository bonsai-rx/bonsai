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
    public class ContoursVisualizer : IplImageVisualizer
    {
        public override void Show(object value)
        {
            var contours = (Contours)value;
            var output = new IplImage(contours.ImageSize, 8, 1);
            output.SetZero();

            if (!contours.FirstContour.IsInvalid)
            {
                Core.cvDrawContours(output, contours.FirstContour, CvScalar.All(255), CvScalar.All(0), 1, -1, 8, CvPoint.Zero);
            }

            base.Show(output);
        }
    }
}
