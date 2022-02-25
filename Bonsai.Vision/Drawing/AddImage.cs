using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing the specified image to the canvas.
    /// </summary>
    [Description("Draws the specified image to the canvas.")]
    public class AddImage : CanvasElement
    {
        /// <summary>
        /// Gets or sets the image to draw.
        /// </summary>
        [XmlIgnore]
        [Description("The image to draw.")]
        public IplImage Image { get; set; }

        /// <summary>
        /// Gets or sets the optional region in which to draw the image.
        /// </summary>
        [Description("The optional region in which to draw the image.")]
        public Rect Destination { get; set; }

        /// <summary>
        /// Gets or sets the interpolation method used to resize the input image, if required.
        /// </summary>
        [Description("The interpolation method used to resize the input image, if required.")]
        public SubPixelInterpolation Interpolation { get; set; } = SubPixelInterpolation.Linear;

        /// <summary>
        /// Returns the image drawing operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var input = Image;
            var destination = Destination;
            var interpolation = Interpolation;
            return image =>
            {
                if (input != null)
                {
                    if (destination.Width > 0 && destination.Height > 0) image = image.GetSubRect(destination);
                    if (input.Size != image.Size) CV.Resize(input, image, interpolation);
                    else CV.Copy(input, image);
                }
            };
        }
    }
}
