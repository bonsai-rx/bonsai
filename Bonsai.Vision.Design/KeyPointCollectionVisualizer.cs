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
            var output = new IplImage(keyPoints.Image.Size, IplDepth.U8, 3);
            CV.CvtColor(keyPoints.Image, output, ColorConversion.Gray2Bgr);

            foreach (var keyPoint in keyPoints)
            {
                CV.Circle(output, new Point(keyPoint), 2, Scalar.Rgb(255, 0, 0), -1);
            }

            base.Show(output);
        }
    }
}
