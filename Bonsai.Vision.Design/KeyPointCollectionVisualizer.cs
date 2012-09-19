using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Vision;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(KeyPointCollectionVisualizer), Target = typeof(KeyPointCollection))]

namespace Bonsai.Vision.Design
{
    public class KeyPointCollectionVisualizer : IplImageVisualizer
    {
        public override void Show(object value)
        {
            var keyPoints = (KeyPointCollection)value;
            var output = new IplImage(keyPoints.Image.Size, 8, 3);
            ImgProc.cvCvtColor(keyPoints.Image, output, ColorConversion.GRAY2BGR);

            foreach (var keyPoint in keyPoints)
            {
                Core.cvCircle(output, new CvPoint(keyPoint.Point), 2, CvScalar.Rgb(255, 0, 0), -1, 8, 0);
            }

            base.Show(output);
        }
    }
}
