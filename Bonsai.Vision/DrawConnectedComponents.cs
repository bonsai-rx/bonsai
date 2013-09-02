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
            var output = new IplImage(input.ImageSize, IplDepth.U8, 1);
            output.SetZero();

            foreach (var component in input)
            {
                CV.DrawContours(output, component.Contour, Scalar.All(255), Scalar.All(0), 0, -1);
            }

            return output;
        }
    }
}
