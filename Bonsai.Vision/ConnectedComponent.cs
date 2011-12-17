using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ConnectedComponent
    {
        public CvPoint Position { get; set; }

        public float Orientation { get; set; }

        public CvContour Contour { get; set; }
    }
}
