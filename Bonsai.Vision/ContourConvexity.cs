using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    public class ContourConvexity
    {
        public ContourConvexity(Contour contour, Seq convexHull, Seq convexityDefects)
        {
            Contour = contour;
            ConvexHull = convexHull;
            ConvexityDefects = convexityDefects;
        }

        public Contour Contour { get; private set; }

        public Seq ConvexHull { get; private set; }

        public Seq ConvexityDefects { get; private set; }
    }
}
