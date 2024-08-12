using System.Drawing;

namespace Bonsai.NuGet.Design
{
    static class GraphicsHelper
    {
        public static SizeF GetImageSize(this Graphics graphics, Image image)
        {
            return new(
                width: image.Width * graphics.DpiX / image.HorizontalResolution,
                height: image.Height * graphics.DpiY / image.VerticalResolution);
        }
    }
}
