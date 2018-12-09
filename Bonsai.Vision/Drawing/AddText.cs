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

        internal override void Draw(IplImage image, Graphics graphics, Brush brush, StringFormat format)
        {
            var origin = Origin;
            graphics.DrawString(Text, Font, brush, origin.X, origin.Y, format);
        }
    }
}
