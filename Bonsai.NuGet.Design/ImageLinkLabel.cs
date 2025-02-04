using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    internal class ImageLinkLabel : LinkLabel
    {
        private Image baseImage;

        public ImageLinkLabel()
        {
            ImageAlign = ContentAlignment.MiddleLeft;
            TextAlign = ContentAlignment.MiddleRight;
        }

        public new Image Image
        {
            get => baseImage;
            set
            {
                baseImage = value;
                if (!NativeMethods.IsRunningOnMono)
                    base.Image = baseImage;

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

        protected override void OnPaint(PaintEventArgs e)
        {
            var rectangle = ClientRectangle;
            base.OnPaintBackground(e);
            if (Image != null)
            {
                var imageBounds = CalcImageRenderBounds(Image, rectangle, ImageAlign);
                if (NativeMethods.IsRunningOnMono)
                    e.Graphics.DrawImage(Image, imageBounds.Location);
                else
                    rectangle.X += imageBounds.Width / 2 - imageBounds.X;
                rectangle.X += imageBounds.Width / 2;
            }

            TextRenderer.DrawText(e.Graphics, Text, Font, rectangle, LinkColor);
        }
    }
}
