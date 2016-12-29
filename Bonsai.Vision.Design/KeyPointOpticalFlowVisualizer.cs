using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Vision;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(KeyPointOpticalFlowVisualizer), Target = typeof(KeyPointOpticalFlow))]

namespace Bonsai.Vision.Design
{
    public class KeyPointOpticalFlowVisualizer : IplImageVisualizer
    {
        const float DefaultHeight = 480;
        const int DefaultThickness = 2;

        internal static void Draw(IplImage image, KeyPointOpticalFlow tracking)
        {
            if (image != null)
            {
                var previous = tracking.PreviousKeyPoints;
                var current = tracking.CurrentKeyPoints;
                var color = image.Channels == 1 ? Scalar.Real(255) : Scalar.Rgb(255, 0, 0);
                var thickness = DefaultThickness * (int)Math.Ceiling(image.Height / DefaultHeight);
                for (int i = 0; i < previous.Count; i++)
                {
                    var previousPoint = new Point(previous[i]);
                    var currentPoint = new Point(current[i]);
                    CV.Line(image, previousPoint, currentPoint, Scalar.Rgb(255, 0, 0), thickness);
                }
            }
        }

        public override void Show(object value)
        {
            var tracking = (KeyPointOpticalFlow)value;
            var image = tracking.CurrentKeyPoints.Image;
            var output = new IplImage(image.Size, IplDepth.U8, 3);
            CV.CvtColor(image, output, ColorConversion.Gray2Bgr);
            Draw(output, tracking);
            base.Show(output);
        }
    }
}
