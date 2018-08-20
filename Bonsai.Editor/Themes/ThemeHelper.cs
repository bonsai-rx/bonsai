using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor.Themes
{
    class ThemeHelper
    {
        const int E = ushort.MaxValue + 1;

        // Integer-based RGB to HSV conversion (Chernov et al. 2015)
        static void RgbToHsv(byte r, byte g, byte b, out int hue, out ushort saturation, out byte value)
        {
            var min = r;
            var mid = g;
            var max = b;
            var I = 3;
            if (min > mid) { mid = r; min = g; I = 4; }
            if (mid > max)
            {
                max = mid;
                mid = b;
                if (min > mid)
                {
                    mid = min;
                    min = b;
                    I = max == g ? 1 : 0;
                }
                else { I = max == g ? 2 : 5; }
            }

            value = max;
            var chroma = max - min;
            if (chroma == 0) hue = saturation = 0;
            else
            {
                saturation = (ushort)(((chroma << 16) - 1) / value);
                var f = ((mid - min) << 16) / chroma + 1;
                if (I % 2 != 0) f = E - f;
                hue = E * I + f;
            }
        }

        // Integer-based HSV to RGB conversion (Chernov et al. 2015)
        static void HsvToRgb(int hue, ushort saturation, byte value, out byte r, out byte g, out byte b)
        {
            if (saturation == 0 || value == 0) r = g = b = value;
            else
            {
                var chroma = ((saturation * value) >> 16) + 1;
                var min = value - chroma;
                var I = hue / E;
                var f = hue - E * I;
                if (I % 2 != 0) f = E - f;
                var mid = ((f * chroma) >> 16) + min;
                switch (I)
                {
                    case 0: r = value; g = (byte)mid; b = (byte)min; break;
                    case 1: r = (byte)mid; g = value; b = (byte)min; break;
                    case 2: r = (byte)min; g = value; b = (byte)mid; break;
                    case 3: r = (byte)min; g = (byte)mid; b = value; break;
                    case 4: r = (byte)mid; g = (byte)min; b = value; break;
                    case 5: r = value; g = (byte)min; b = (byte)mid; break;
                    default: throw new InvalidOperationException("Invalid HSV value.");
                }
            }
        }

        static void Invert(ref byte r, ref byte g, ref byte b)
        {
            // Invert colors
            r = (byte)(255 - r);
            g = (byte)(255 - g);
            b = (byte)(255 - b);

            // Rgb-to-Hsv
            int hue;
            ushort saturation;
            byte value;
            RgbToHsv(r, g, b, out hue, out saturation, out value);

            // Rotate hue by 180-degrees
            const int Rotation = 3 * E;
            hue = (hue + Rotation) % (6 * E);

            // Hsv-to-Rgb
            HsvToRgb(hue, saturation, value, out r, out g, out b);
        }

        public static Color Invert(Color color)
        {
            var r = color.R;
            var g = color.G;
            var b = color.B;
            Invert(ref r, ref g, ref b);
            return Color.FromArgb(r, g, b);
        }

        public static Image Invert(Image image)
        {
            var result = new Bitmap(image);
            var pixelRange = new Rectangle(0, 0, result.Width, result.Height);
            var pixelData = result.LockBits(pixelRange, ImageLockMode.ReadWrite, result.PixelFormat);
            try
            {
                var values = new byte[pixelData.Stride * pixelData.Height];
                Marshal.Copy(pixelData.Scan0, values, 0, values.Length);
                for (int i = 0; i < values.Length; i += 4)
                {
                    Invert(ref values[i + 2],
                           ref values[i + 1],
                           ref values[i + 0]);
                }
                Marshal.Copy(values, 0, pixelData.Scan0, values.Length);
            }
            finally { result.UnlockBits(pixelData); }
            return result;
        }

        public static Image InvertScale(Image image, Size size, Color backColor)
        {
            using (var inverted = Invert(image))
            {
                var result = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
                using (var graphics = Graphics.FromImage(result))
                {
                    graphics.DrawImage(inverted, 0, 0, result.Width, result.Height);
                }
                return result;
            }
        }
    }
}
