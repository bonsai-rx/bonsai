using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public static class ExtensionMethods
    {
        public static IplDepth GetConversionDepth(this ColorConversion conversion)
        {
            switch (conversion)
            {
                case ColorConversion.Bgr2Bgr565:
                case ColorConversion.Rgb2Bgr565:
                case ColorConversion.Bgra2Bgr565:
                case ColorConversion.Rgba2Bgr565:
                case ColorConversion.Gray2Bgr565:
                case ColorConversion.Bgr2Bgr555:
                case ColorConversion.Rgb2Bgr555:
                case ColorConversion.Bgra2Bgr555:
                case ColorConversion.Rgba2Bgr555:
                case ColorConversion.Gray2Bgr555: return IplDepth.U16;
                default: return IplDepth.U8;
            }
        }

        public static int GetConversionNumChannels(this ColorConversion conversion)
        {
            switch (conversion)
            {
                case ColorConversion.Bgr2Rgba:
                case ColorConversion.Bgra2Rgba:
                case ColorConversion.Gray2Bgra:
                case ColorConversion.Bgr5652Bgra:
                case ColorConversion.Bgr5652Rgba:
                case ColorConversion.Bgr5552Bgra:
                case ColorConversion.Bgr5552Rgba:
                case ColorConversion.Lab2LBgr:
                case ColorConversion.Lab2LRgb:
                case ColorConversion.Luv2LBgr:
                case ColorConversion.Luv2LRgb:
                case ColorConversion.Rgba2mRgba:
                case ColorConversion.mRgba2Rgba:
                case ColorConversion.Bgr2Bgra: return 4;
                case ColorConversion.Rgba2Bgr:
                case ColorConversion.Bgr2Rgb:
                case ColorConversion.Gray2Bgr:
                case ColorConversion.Bgr5652Bgr:
                case ColorConversion.Bgr5652Rgb:
                case ColorConversion.Bgr2Xyz:
                case ColorConversion.Rgb2Xyz:
                case ColorConversion.Xyz2Bgr:
                case ColorConversion.Xyz2Rgb:
                case ColorConversion.Bgr2YCrCb:
                case ColorConversion.Rgb2YCrCb:
                case ColorConversion.YCrCb2Bgr:
                case ColorConversion.YCrCb2Rgb:
                case ColorConversion.Bgr2Hsv:
                case ColorConversion.Rgb2Hsv:
                case ColorConversion.BayerBG2Bgr:
                case ColorConversion.BayerGB2Bgr:
                case ColorConversion.BayerRG2Bgr:
                case ColorConversion.BayerGR2Bgr:
                case ColorConversion.Bgr5552Bgr:
                case ColorConversion.Bgr5552Rgb:
                case ColorConversion.Hsv2Bgr:
                case ColorConversion.Hsv2Rgb:
                case ColorConversion.Lab2Bgr:
                case ColorConversion.Lab2Rgb:
                case ColorConversion.Luv2Bgr:
                case ColorConversion.Luv2Rgb:
                case ColorConversion.Hls2Bgr:
                case ColorConversion.Hls2Rgb:
                case ColorConversion.Bgr2Hls:
                case ColorConversion.Rgb2Hls:
                case ColorConversion.Bgr2HsvFull:
                case ColorConversion.Rgb2HsvFull:
                case ColorConversion.Bgr2HlsFull:
                case ColorConversion.Rgb2HlsFull:
                case ColorConversion.Hsv2BgrFull:
                case ColorConversion.Hsv2RgbFull:
                case ColorConversion.Hls2BgrFull:
                case ColorConversion.Hls2RgbFull:
                case ColorConversion.Bgr2Yuv:
                case ColorConversion.Rgb2Yuv:
                case ColorConversion.Yuv2Bgr:
                case ColorConversion.Yuv2Rgb:
                case ColorConversion.BayerBG2BgrVng:
                case ColorConversion.BayerGB2BgrVng:
                case ColorConversion.BayerRG2BgrVng:
                case ColorConversion.BayerGR2BgrVng:
                case ColorConversion.Bgr2Lab:
                case ColorConversion.Rgb2Lab:
                case ColorConversion.LBgr2Lab:
                case ColorConversion.LRgb2Lab:
                case ColorConversion.Bgr2Luv:
                case ColorConversion.Rgb2Luv:
                case ColorConversion.LBgr2Luv:
                case ColorConversion.LRgb2Luv:
                case ColorConversion.Bgra2Bgr: return 3;
                case ColorConversion.Rgb2Gray:
                case ColorConversion.Bgra2Gray:
                case ColorConversion.Rgba2Gray:
                case ColorConversion.Bgr5652Gray:
                case ColorConversion.Bgr5552Gray:
                case ColorConversion.Bgr2Bgr565:
                case ColorConversion.Rgb2Bgr565:
                case ColorConversion.Bgra2Bgr565:
                case ColorConversion.Rgba2Bgr565:
                case ColorConversion.Gray2Bgr565:
                case ColorConversion.Bgr2Bgr555:
                case ColorConversion.Rgb2Bgr555:
                case ColorConversion.Bgra2Bgr555:
                case ColorConversion.Rgba2Bgr555:
                case ColorConversion.Gray2Bgr555:
                case ColorConversion.BayerBG2Gray:
                case ColorConversion.BayerGB2Gray:
                case ColorConversion.BayerGR2Gray:
                case ColorConversion.BayerRG2Gray:
                case ColorConversion.Yuv2Gray420:
                case ColorConversion.Yuv2GrayUyvy:
                case ColorConversion.Yuv2GrayYuy2:
                case ColorConversion.Bgr2Gray: return 1;
                default: throw new ArgumentException("Unsupported color conversion code.");
            }
        }

        public static bool FormatEquals(this IplImage image, IplImage other)
        {
            return image.Width == other.Width && image.Height == other.Height &&
                   image.Depth == other.Depth && image.Channels == other.Channels;
        }
    }
}
