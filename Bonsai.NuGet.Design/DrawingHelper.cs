using System.Drawing;
using System.Drawing.Drawing2D;

namespace Bonsai.NuGet.Design
{
    static class DrawingHelper
    {
        public static SizeF GetImageSize(this Graphics graphics, Image image)
        {
            return new(
                width: image.Width * graphics.DpiX / image.HorizontalResolution,
                height: image.Height * graphics.DpiY / image.VerticalResolution);
        }

        public static Bitmap Resize(this Image image, Size newSize)
        {
            var result = new Bitmap(newSize.Width, newSize.Height);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newSize.Width, newSize.Height);
            }
            return result;
        }
    }
}
