using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class RegionActivity
    {
        public CvPoint[] Roi { get; set; }

        public CvRect Rect { get; set; }

        public CvScalar Activity { get; set; }
    }
}
