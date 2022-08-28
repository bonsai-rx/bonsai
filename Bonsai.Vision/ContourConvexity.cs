using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Provides the result of a convexity analysis of a polygonal contour,
    /// representing the deviations between each point in the contour and its
    /// convex hull.
    /// </summary>
    /// <seealso cref="Vision.ConvexityDefects"/>
    public class ContourConvexity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContourConvexity"/> class
        /// using the specified contour, convex hull and corresponding convexity defects.
        /// </summary>
        /// <param name="contour">
        /// The polygonal contour from which the convex hull and convexity defects
        /// were calculated.
        /// </param>
        /// <param name="convexHull">
        /// A sequence containing the points in the convex hull of the polygonal contour.
        /// </param>
        /// <param name="convexityDefects">
        /// A sequence of <see cref="ConvexityDefect"/> structures representing the
        /// deviations between each point in the polygonal contour and its convex hull.
        /// </param>
        public ContourConvexity(Contour contour, Seq convexHull, Seq convexityDefects)
        {
            Contour = contour;
            ConvexHull = convexHull;
            ConvexityDefects = convexityDefects;
        }

        /// <summary>
        /// Gets the polygonal contour from which the convex hull and convexity defects
        /// were calculated.
        /// </summary>
        public Contour Contour { get; private set; }

        /// <summary>
        /// Gets a sequence containing the points in the convex hull of the polygonal contour.
        /// </summary>
        public Seq ConvexHull { get; private set; }

        /// <summary>
        /// Gets a sequence of <see cref="ConvexityDefect"/> structures representing the
        /// deviations between each point in the polygonal contour and its convex hull.
        /// </summary>
        public Seq ConvexityDefects { get; private set; }
    }
}
