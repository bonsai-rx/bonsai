using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    [Description("Renders the specified image to the canvas.")]
    public class AddImage : CanvasElement
    {
        public AddImage()
        {
            Interpolation = SubPixelInterpolation.Linear;
        }

        [XmlIgnore]
        [Description("The image to draw.")]
        public IplImage Image { get; set; }

        [Description("The optional region in which to draw the image.")]
        public Rect Destination { get; set; }

        [Description("The interpolation method used to resize the input image, if necessary.")]
        public SubPixelInterpolation Interpolation { get; set; }

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
