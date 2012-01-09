using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Contours
    {
        public Contours(CvSeq firstContour, CvSize imageSize)
        {
            FirstContour = firstContour;
            ImageSize = imageSize;
        }

        public CvSeq FirstContour { get; private set; }

        public CvSize ImageSize { get; private set; }
    }
}
