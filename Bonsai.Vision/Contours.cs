using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Provides a reference to a hierarchy of polygonal contours extracted
    /// from an image bitmap.
    /// </summary>
    public class Contours
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contours"/> class
        /// using the specified root node and image size.
        /// </summary>
        /// <param name="firstContour">
        /// The first node in the hierarchy of extracted polygonal contours.
        /// </param>
        /// <param name="imageSize">
        /// The pixel-accurate size of the image from which the contours
        /// were extracted.
        /// </param>
        public Contours(Seq firstContour, Size imageSize)
        {
            FirstContour = firstContour;
            ImageSize = imageSize;
        }

        /// <summary>
        /// Gets the reference to the first polygonal contour in the hierarchy.
        /// </summary>
        public Seq FirstContour { get; private set; }

        /// <summary>
        /// Gets the pixel-accurate size of the image from which the polygonal
        /// contours were extracted.
        /// </summary>
        public Size ImageSize { get; private set; }
    }
}
