using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Finds the convexity defects of a contour.")]
    public class ConvexityDefects : Transform<Seq, ContourConvexity>
    {
        static ContourConvexity ProcessContour(Seq contour)
        {
            return ProcessContour(Contour.FromSeq(contour));
        }

        static ContourConvexity ProcessContour(Contour contour)
        {
            Seq convexHull = null;
            Seq convexityDefects = null;
            if (contour != null)
            {
                var convexHullIndices = CV.ConvexHull2(contour);
                convexHull = CV.ConvexHull2(contour, returnPoints: true);
                convexityDefects = CV.ConvexityDefects(contour, convexHullIndices);
            }

            return new ContourConvexity(contour, convexHull, convexityDefects);
        }

        public override IObservable<ContourConvexity> Process(IObservable<Seq> source)
        {
            return source.Select(ProcessContour);
        }

        public IObservable<ContourConvexity> Process(IObservable<ConnectedComponent> source)
        {
            return source.Select(input => ProcessContour(input.Contour));
        }
    }
}
