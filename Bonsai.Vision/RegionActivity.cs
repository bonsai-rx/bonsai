using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents information about activity detected inside a specified polygonal
    /// region of interest.
    /// </summary>
    public class RegionActivity
    {
        /// <summary>
        /// Gets or sets the array of vertices specifying the polygonal region
        /// of interest.
        /// </summary>
        public Point[] Roi { get; set; }

        /// <summary>
        /// Gets or sets the bounding rectangle of the region of interest.
        /// </summary>
        public Rect Rect { get; set; }

        /// <summary>
        /// Gets or sets the total per-channel activity of pixels in the
        /// region of interest.
        /// </summary>
        public Scalar Activity { get; set; }
    }
}
