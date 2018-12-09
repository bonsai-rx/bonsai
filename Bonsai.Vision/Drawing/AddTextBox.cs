using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision.Drawing
{
    [Description("Renders text strokes with the specified font and color inside the specified layout rectangle.")]
    public class AddTextBox : AddTextBase
    {
        [Description("The optional region in which to draw the text. By default the box will fill the entire image.")]
        public Rect Destination { get; set; }

        internal override void Draw(IplImage image, Graphics graphics, Brush brush, StringFormat format)
        {
            var rect = Destination;
            if (rect.Width == 0) rect.Width = image.Width;
            if (rect.Height == 0) rect.Height = image.Height;
            graphics.DrawString(Text, Font, brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), format);
        }
    }
}
