using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Approximates polygonal curve(s) with the specified precision.")]
    public class ApproximatePolygon : Transform<Contours, Contours>
    {
        [Description("The polygon approximation method.")]
        public PolygonApproximation Method { get; set; }

        [Description("The desired approximation accuracy.")]
        public double Eps { get; set; }

        [Description("Specifies whether approximation should proceed for all hierarchical levels.")]
        public bool Recursive { get; set; }

        public override IObservable<Contours> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                Seq output = input.FirstContour;
                if (output != null)
                {
                    output = CV.ApproxPoly(output, Contour.HeaderSize, output.Storage, Method, Eps, Recursive);
                }

                return new Contours(output, input.ImageSize);
            });
        }
    }
}
