using OpenCV.Net;
using System;
using System.ComponentModel;
using Point = OpenCV.Net.Point;

namespace Bonsai.Vision.Drawing
{
    [Description("Renders text strokes with the specified font and color at a given location.")]
    public class AddText : AddTextBase
    {
        [Description("The coordinates of the upper-left corner of the drawn text.")]
        public Point Origin { get; set; }

        protected override Action<IplImage> GetRenderer()
        {
            return GetRenderer(Origin, (image, graphics, text, font, brush, format, origin) =>
            {
                graphics.DrawString(text, font, brush, origin.X, origin.Y, format);
            });
        }
    }
}
