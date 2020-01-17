#if !NETSTANDARD2_1
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
#endif