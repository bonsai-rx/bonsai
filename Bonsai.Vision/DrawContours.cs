using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class DrawContours : Projection<Contours, IplImage>
    {
        public override IplImage Process(Contours input)
        {
            var output = new IplImage(input.ImageSize, 8, 1);
            output.SetZero();

            if (!input.FirstContour.IsInvalid)
            {
                Core.cvDrawContours(output, input.FirstContour, CvScalar.All(255), CvScalar.All(0), 1, -1, 8, CvPoint.Zero);
            }

            return output;
        }
    }
}
