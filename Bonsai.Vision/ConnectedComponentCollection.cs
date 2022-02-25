using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a collection of connected components.
    /// </summary>
    public class ConnectedComponentCollection : Collection<ConnectedComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentCollection"/>
        /// class using the specified image size.
        /// </summary>
        /// <param name="imageSize">
        /// The size of the image from which the connected components were extracted.
        /// </param>
        public ConnectedComponentCollection(Size imageSize)
        {
            ImageSize = imageSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentCollection"/>
        /// class as a wrapper for the specified list of connected components.
        /// </summary>
        /// <param name="components">
        /// The list of connected components that is wrapped by the new collection.
        /// </param>
        /// <param name="imageSize">
        /// The size of the image from which the connected components were extracted.
        /// </param>
        public ConnectedComponentCollection(IList<ConnectedComponent> components, Size imageSize)
            : base(components)
        {
            ImageSize = imageSize;
        }

        /// <summary>
        /// Gets the pixel-accurate size of the image from which the connected
        /// components were extracted.
        /// </summary>
        public Size ImageSize { get; private set; }
    }
}
