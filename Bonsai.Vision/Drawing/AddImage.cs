using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected override void Draw(IplImage image)
        {
            var input = Image;
            if (input != null)
            {
                var destination = Destination;
                if (destination.Width > 0 && destination.Height > 0) image = image.GetSubRect(destination);
                if (input.Size != image.Size) CV.Resize(input, image, Interpolation);
                else CV.Copy(input, image);
            }
        }
    }
}
