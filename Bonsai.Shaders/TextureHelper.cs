using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class TextureHelper
    {
        public static void UpdateTexture(int texture, IplImage image)
        {
            if (image == null) throw new ArgumentNullException("image");
            PixelFormat pixelFormat;
            switch (image.Channels)
            {
                case 1: pixelFormat = PixelFormat.Luminance; break;
                case 2: pixelFormat = PixelFormat.Rg; break;
                case 3: pixelFormat = PixelFormat.Bgr; break;
                case 4: pixelFormat = PixelFormat.Bgra; break;
                default: throw new ArgumentException("Image has an unsupported number of channels.", "image");
            }

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, image.WidthStep / image.Channels);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, pixelFormat, PixelType.UnsignedByte, image.ImageData);
        }
    }
}
