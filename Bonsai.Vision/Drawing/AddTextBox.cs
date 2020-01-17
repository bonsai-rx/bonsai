#if !NETSTANDARD2_1
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

        protected override Action<IplImage> GetRenderer()
        {
            return GetRenderer(Destination, (image, graphics, text, font, brush, format, rect) =>
            {
                if (rect.Width == 0) rect.Width = image.Width;
                if (rect.Height == 0) rect.Height = image.Height;
                graphics.DrawString(text, font, brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), format);
            });
        }
    }
}
#endif