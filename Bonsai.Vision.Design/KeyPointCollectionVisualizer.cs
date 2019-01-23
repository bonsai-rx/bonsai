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
        const float DefaultHeight = 480;
        const int DefaultRadius = 2;

        internal static void Draw(IplImage image, KeyPointCollection keyPoints)
        {
            if (image != null)
            {
                var color = image.Channels == 1 ? Scalar.Real(255) : Scalar.Rgb(255, 0, 0);
                var radius = DefaultRadius * (int)Math.Ceiling(image.Height / DefaultHeight);
                foreach (var keyPoint in keyPoints)
                {
                    CV.Circle(image, new Point(keyPoint), radius, color, -1);
                }
            }
        }

        public override void Show(object value)
        {
            var keyPoints = (KeyPointCollection)value;
            var image = keyPoints.Image;
            var output = new IplImage(image.Size, IplDepth.U8, 3);
            if (image.Channels == 1) CV.CvtColor(image, output, ColorConversion.Gray2Bgr);
            else CV.Copy(image, output);
            Draw(output, keyPoints);
            base.Show(output);
        }
    }
}
