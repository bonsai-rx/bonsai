using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ApproximatePolygon : Transform<Contours, Contours>
    {
        public PolygonApproximation Method { get; set; }

        public double Eps { get; set; }

        public bool Recursive { get; set; }

        public override IObservable<Contours> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                Seq output = input.FirstContour;
                if (!input.FirstContour.IsInvalid)
                {
                    output = CV.ApproxPoly(input.FirstContour, Contour.HeaderSize, input.FirstContour.Storage, Method, Eps, Recursive);
                }

                return new Contours(output, input.ImageSize);
            });
        }
    }
}
