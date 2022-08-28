using OpenCV.Net;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a collection of property values that will be assigned
    /// to the specified camera or video file upon initialization.
    /// </summary>
    [TypeConverter("Bonsai.Vision.Design.CapturePropertyCollectionConverter, Bonsai.Vision.Design")]
    public class CapturePropertyCollection : Collection<CapturePropertyAssignment>
    {
        /// <summary>
        /// Gets the video capture stream that the properties will be assigned to.
        /// </summary>
        public Capture Capture { get; internal set; }
    }
}
