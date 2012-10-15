using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ApproximatePolygon : Transform<Contours, Contours>
    {
        public PolygonApproximation Method { get; set; }

        public double Eps { get; set; }

        public int Recursive { get; set; }

        public override Contours Process(Contours input)
        {
            CvSeq output = input.FirstContour;
            if (!input.FirstContour.IsInvalid)
            {
                output = ImgProc.cvApproxPoly(input.FirstContour, CvContour.HeaderSize, input.FirstContour.Storage, Method, Eps, Recursive);
            }

            return new Contours(output, input.ImageSize);
        }
    }
}
