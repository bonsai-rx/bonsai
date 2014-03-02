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
            if (contour == null)
            {
                throw new ArgumentNullException("contour");
            }

            if (convexHull == null)
            {
                throw new ArgumentNullException("convexHull");
            }

            if (convexityDefects == null)
            {
                throw new ArgumentNullException("convexityDefects");
            }

            Contour = contour;
            ConvexHull = convexHull;
            ConvexityDefects = convexityDefects;
        }

        public Contour Contour { get; private set; }

        public Seq ConvexHull { get; private set; }

        public Seq ConvexityDefects { get; private set; }
    }
}
