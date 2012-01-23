using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ConnectedComponent
    {
        public CvPoint Center { get; set; }

        public double Angle { get; set; }

        public double Area { get; set; }

        public CvContour Contour { get; set; }
    }
}
