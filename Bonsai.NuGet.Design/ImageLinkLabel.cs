using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    internal class ImageLinkLabel : LinkLabel
    {
        public ImageLinkLabel()
        {
            ImageAlign = ContentAlignment.MiddleLeft;
            TextAlign = ContentAlignment.MiddleRight;
        }

        public new Image Image
        {
            get => base.Image;
            set
            {
                base.Image = value;
                if (AutoSize)
                {
                    // force size calculation
                    AutoSize = false;
                    AutoSize = true;
                }
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var size = base.GetPreferredSize(proposedSize);
            if (Image != null)
            {
                using var graphics = CreateGraphics();
                var imageSize = Size.Ceiling(graphics.GetImageSize(Image));
                size.Width += imageSize.Width;
                size.Height = Math.Max(size.Height, imageSize.Height);
            }
            return size;
        }
    }
}
