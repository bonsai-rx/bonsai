using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public static class ExtensionMethods
    {
        public static int GetConversionDepth(this ColorConversion conversion)
        {
            switch (conversion)
            {
                case ColorConversion.BGR2BGR565:
                case ColorConversion.RGB2BGR565:
                case ColorConversion.BGRA2BGR565:
                case ColorConversion.RGBA2BGR565:
                case ColorConversion.GRAY2BGR565:
                case ColorConversion.BGR2BGR555:
                case ColorConversion.RGB2BGR555:
                case ColorConversion.BGRA2BGR555:
                case ColorConversion.RGBA2BGR555:
                case ColorConversion.GRAY2BGR555: return 16;
                default: return 8;
            }
        }

        public static int GetConversionNumChannels(this ColorConversion conversion)
        {
            switch (conversion)
            {
                case ColorConversion.BGR2RGBA:
                case ColorConversion.BGRA2RGBA:
                case ColorConversion.GRAY2BGRA:
                case ColorConversion.BGR5652BGRA:
                case ColorConversion.BGR5652RGBA:
                case ColorConversion.BGR5552BGRA:
                case ColorConversion.BGR5552RGBA:
                case ColorConversion.Lab2LBGR:
                case ColorConversion.Lab2LRGB:
                case ColorConversion.Luv2LBGR:
                case ColorConversion.Luv2LRGB:
                case ColorConversion.BGR2BGRA: return 4;
                case ColorConversion.RGBA2BGR:
                case ColorConversion.BGR2RGB:
                case ColorConversion.GRAY2BGR:
                case ColorConversion.BGR5652BGR:
                case ColorConversion.BGR5652RGB:
                case ColorConversion.BGR2XYZ:
                case ColorConversion.RGB2XYZ:
                case ColorConversion.XYZ2BGR:
                case ColorConversion.XYZ2RGB:
                case ColorConversion.BGR2YCrCb:
                case ColorConversion.RGB2YCrCb:
                case ColorConversion.YCrCb2BGR:
                case ColorConversion.YCrCb2RGB:
                case ColorConversion.BGR2HSV:
                case ColorConversion.RGB2HSV:
                case ColorConversion.BayerBG2BGR:
                case ColorConversion.BayerGB2BGR:
                case ColorConversion.BayerRG2BGR:
                case ColorConversion.BayerGR2BGR:
                case ColorConversion.BGR5552BGR:
                case ColorConversion.BGR5552RGB:
                case ColorConversion.HSV2BGR:
                case ColorConversion.HSV2RGB:
                case ColorConversion.Lab2BGR:
                case ColorConversion.Lab2RGB:
                case ColorConversion.Luv2BGR:
                case ColorConversion.Luv2RGB:
                case ColorConversion.HLS2BGR:
                case ColorConversion.HLS2RGB:
                case ColorConversion.BGR2HLS:
                case ColorConversion.RGB2HLS:
                case ColorConversion.BGR2HSV_FULL:
                case ColorConversion.RGB2HSV_FULL:
                case ColorConversion.BGR2HLS_FULL:
                case ColorConversion.RGB2HLS_FULL:
                case ColorConversion.HSV2BGR_FULL:
                case ColorConversion.HSV2RGB_FULL:
                case ColorConversion.HLS2BGR_FULL:
                case ColorConversion.HLS2RGB_FULL:
                case ColorConversion.BGR2YUV:
                case ColorConversion.RGB2YUV:
                case ColorConversion.YUV2BGR:
                case ColorConversion.YUV2RGB:
                case ColorConversion.BayerBG2BGR_VNG:
                case ColorConversion.BayerGB2BGR_VNG:
                case ColorConversion.BayerRG2BGR_VNG:
                case ColorConversion.BayerGR2BGR_VNG:
                case ColorConversion.BGR2Lab:
                case ColorConversion.RGB2Lab:
                case ColorConversion.LBGR2Lab:
                case ColorConversion.LRGB2Lab:
                case ColorConversion.BGR2Luv:
                case ColorConversion.RGB2Luv:
                case ColorConversion.LBGR2Luv:
                case ColorConversion.LRGB2Luv:
                case ColorConversion.BGRA2BGR: return 3;
                case ColorConversion.RGB2GRAY:
                case ColorConversion.BGRA2GRAY:
                case ColorConversion.RGBA2GRAY:
                case ColorConversion.BGR5652GRAY:
                case ColorConversion.BGR5552GRAY:
                case ColorConversion.BGR2BGR565:
                case ColorConversion.RGB2BGR565:
                case ColorConversion.BGRA2BGR565:
                case ColorConversion.RGBA2BGR565:
                case ColorConversion.GRAY2BGR565:
                case ColorConversion.BGR2BGR555:
                case ColorConversion.RGB2BGR555:
                case ColorConversion.BGRA2BGR555:
                case ColorConversion.RGBA2BGR555:
                case ColorConversion.GRAY2BGR555:
                case ColorConversion.BGR2GRAY: return 1;
                default: throw new ArgumentException("Unsupported color conversion code.");
            }
        }

        public static bool FormatEquals(this IplImage image, IplImage other)
        {
            return image.Width == other.Width && image.Height == other.Height &&
                   image.Depth == other.Depth && image.NumChannels == other.NumChannels;
        }
    }
}
