using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class RegionActivity
    {
        public Point[] Roi { get; set; }

        public Rect Rect { get; set; }

        public Scalar Activity { get; set; }
    }
}
