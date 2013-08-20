using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class DrawContours : Selector<Contours, IplImage>
    {
        public DrawContours()
        {
            MaxLevel = 1;
            Thickness = -1;
        }

        public int MaxLevel { get; set; }

        public int Thickness { get; set; }

        public override IplImage Process(Contours input)
        {
            var output = new IplImage(input.ImageSize, 8, 1);
            output.SetZero();

            if (!input.FirstContour.IsInvalid)
            {
                Core.cvDrawContours(output, input.FirstContour, CvScalar.All(255), CvScalar.All(0), MaxLevel, Thickness, 8, CvPoint.Zero);
            }

            return output;
        }
    }
}
