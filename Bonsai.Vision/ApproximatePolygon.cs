using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ApproximatePolygon : Projection<Contours, Contours>
    {
        public PolygonApproximation Method { get; set; }

        public double Parameter { get; set; }

        public int Parameter2 { get; set; }

        public override Contours Process(Contours input)
        {
            CvSeq output = input.FirstContour;
            if (!input.FirstContour.IsInvalid)
            {
                output = ImgProc.cvApproxPoly(input.FirstContour, CvContour.HeaderSize, input.FirstContour.Storage, Method, Parameter, Parameter2);
            }

            return new Contours(output, input.ImageSize);
        }
    }
}
