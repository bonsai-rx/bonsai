﻿using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders
{
    static class TextureHelper
    {
        static void GetPixelFormat(IplImage image, out PixelFormat pixelFormat, out int pixelSize, out PixelType pixelType)
        {
            switch (image.Channels)
            {
                case 1: pixelFormat = PixelFormat.Luminance; break;
                case 2: pixelFormat = PixelFormat.Rg; break;
                case 3: pixelFormat = PixelFormat.Bgr; break;
                case 4: pixelFormat = PixelFormat.Bgra; break;
                default: throw new ArgumentException("Image has an unsupported number of channels.", nameof(image));
            }

            switch (image.Depth)
            {
                case IplDepth.U8:
                    pixelSize = 1;
                    pixelType = PixelType.UnsignedByte;
                    break;
                case IplDepth.S8:
                    pixelSize = 1;
                    pixelType = PixelType.Byte;
                    break;
                case IplDepth.U16:
                    pixelSize = 2;
                    pixelType = PixelType.UnsignedShort;
                    break;
                case IplDepth.S16:
                    pixelSize = 2;
                    pixelType = PixelType.Short;
                    break;
                case IplDepth.S32:
                    pixelSize = 4;
                    pixelType = PixelType.Int;
                    break;
                case IplDepth.F32:
                    pixelSize = 4;
                    pixelType = PixelType.Float;
                    break;
                default: throw new ArgumentException("Image has an unsupported pixel bit depth.", nameof(image));
            }
        }

        public static void UnpackPixelStore(IplImage image, out PixelFormat pixelFormat, out PixelType pixelType)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            GetPixelFormat(image, out pixelFormat, out int pixelSize, out pixelType);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, image.WidthStep % 4 == 0 ? 4 : 1);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, image.WidthStep / (pixelSize * image.Channels));
        }

        public static void PackPixelStore(IplImage image, out PixelFormat pixelFormat, out PixelType pixelType)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            GetPixelFormat(image, out pixelFormat, out int pixelSize, out pixelType);
            GL.PixelStore(PixelStoreParameter.PackAlignment, image.WidthStep % 4 == 0 ? 4 : 1);
            GL.PixelStore(PixelStoreParameter.PackRowLength, image.WidthStep / (pixelSize * image.Channels));
        }

        public static void UpdateTexture(TextureTarget target, PixelInternalFormat? internalFormat, IplImage image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            UnpackPixelStore(image, out PixelFormat pixelFormat, out PixelType pixelType);
            if (internalFormat.HasValue)
            {
                GL.TexImage2D(target, 0, internalFormat.Value, image.Width, image.Height, 0, pixelFormat, pixelType, image.ImageData);
            }
            else GL.TexSubImage2D(target, 0, 0, 0, image.Width, image.Height, pixelFormat, pixelType, image.ImageData);
            GC.KeepAlive(image);
        }
    }
}
