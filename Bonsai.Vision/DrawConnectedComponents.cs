using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class DrawConnectedComponents : Selector<ConnectedComponentCollection, IplImage>
    {
        public override IplImage Process(ConnectedComponentCollection input)
        {
            var output = new IplImage(input.ImageSize, 8, 1);
            output.SetZero();

            foreach (var component in input)
            {
                Core.cvDrawContours(output, component.Contour, CvScalar.All(255), CvScalar.All(0), 0, -1, 8, CvPoint.Zero);
            }

            return output;
        }
    }
}
