using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds the convexity defects of each contour
    /// in the sequence.
    /// </summary>
    [Description("Finds the convexity defects of each contour in the sequence.")]
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

        /// <summary>
        /// Finds the convexity defects of each contour in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of contours for which to find the convexity defects.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ContourConvexity"/> objects representing the
        /// deviations between each point in the contour and its convex hull.
        /// </returns>
        public override IObservable<ContourConvexity> Process(IObservable<Seq> source)
        {
            return source.Select(ProcessContour);
        }

        /// <summary>
        /// Finds the convexity defects of each <see cref="ConnectedComponent"/>
        /// in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="ConnectedComponent"/> objects containing the
        /// contours for which to find the convexity defects.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ContourConvexity"/> objects representing the
        /// deviations between each point in the contour and its convex hull.
        /// </returns>
        public IObservable<ContourConvexity> Process(IObservable<ConnectedComponent> source)
        {
            return source.Select(input => ProcessContour(input.Contour));
        }
    }
}
