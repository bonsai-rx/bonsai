using System.Collections.ObjectModel;
using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a collection of key points, or features, extracted from a single
    /// image frame.
    /// </summary>
    public class KeyPointCollection : Collection<Point2f>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPointCollection"/> class
        /// with the specified image.
        /// </summary>
        /// <param name="image">
        /// The image from which the key points in the collection were extracted.
        /// </param>
        public KeyPointCollection(IplImage image)
        {
            Image = image;
        }

        /// <summary>
        /// Gets the image from which the key points were extracted.
        /// </summary>
        public IplImage Image { get; private set; }
    }
}
