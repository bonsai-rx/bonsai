using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    internal class ImageLabel : Label
    {
        public ImageLabel()
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

        public new ImageList ImageList { get; set; }

        public new int ImageIndex { get; set; }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var size = base.GetPreferredSize(proposedSize);
            var image = ImageList != null ? ImageList.Images[ImageIndex] : Image;
            if (image != null)
            {
                using var graphics = CreateGraphics();
                var imageSize = Size.Ceiling(graphics.GetImageSize(image));
                size.Width += imageSize.Width;
                size.Height = Math.Max(size.Height, imageSize.Height);
            }
            return size;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ImageList != null)
            {
                var rectangle = ClientRectangle;
                var image = ImageList.Images[ImageIndex];
                var imageBounds = CalcImageRenderBounds(image, rectangle, ImageAlign);
                ImageList.Draw(e.Graphics, imageBounds.Location, ImageIndex);
            }
        }
    }
}
