using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL;

namespace Bonsai.Vision.Design
{
    public class IplImageTexture : IDisposable
    {
        bool disposed;
        int texture;
        int maxTextureSize;
        bool nonPowerOfTwo;
        IplImage textureImage;
        IplImage normalizedImage;

        public IplImageTexture()
        {
            var extensions = GL.GetString(StringName.Extensions).Split(' ');
            nonPowerOfTwo = extensions.Contains("GL_ARB_texture_non_power_of_two");
            GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);

            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        static int NearestPowerOfTwo(int num)
        {
            int n = num > 0 ? num - 1 : 0;

            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;

            if (n != num) n >>= 1;
            return n;
        }

        public void Update(IplImage image)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (image.Depth > IplDepth.U8 && image.Channels == 1)
            {
                double min, max;
                Point minLoc, maxLoc;
                normalizedImage = IplImageHelper.EnsureImageFormat(normalizedImage, image.Size, IplDepth.U8, image.Channels);
                CV.MinMaxLoc(image, out min, out max, out minLoc, out maxLoc);

                var range = max - min;
                var scale = range > 0 ? 255.0 / range : 0;
                var shift = range > 0 ? -min : 0;
                CV.ConvertScale(image, normalizedImage, scale, shift);
                image = normalizedImage;
            }

            if (image.Depth != IplDepth.U8) throw new ArgumentException("Multi-channel floating point or non 8-bit depth images are not supported by the control.", "image");
            if (!nonPowerOfTwo || image.Width > maxTextureSize || image.Height > maxTextureSize)
            {
                var textureWidth = Math.Min(maxTextureSize, NearestPowerOfTwo(image.Width));
                var textureHeight = Math.Min(maxTextureSize, NearestPowerOfTwo(image.Height));
                textureImage = IplImageHelper.EnsureImageFormat(textureImage, new Size(textureWidth, textureHeight), image.Depth, image.Channels);
                CV.Resize(image, textureImage, SubPixelInterpolation.Linear);
                image = textureImage;
            }

            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat;
            switch (image.Channels)
            {
                case 1: pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Luminance; break;
                case 3: pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgr; break;
                case 4: pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra; break;
                default: throw new ArgumentException("Image has an unsupported number of channels.", "image");
            }

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, image.WidthStep / image.Channels);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, pixelFormat, PixelType.UnsignedByte, image.ImageData);
        }

        public void Draw()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1f, -1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1f, -1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1f, 1f);

            GL.End();
        }

        ~IplImageTexture()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GL.DeleteTextures(1, ref texture);
                    if (textureImage != null)
                    {
                        textureImage.Close();
                        textureImage = null;
                    }

                    if (normalizedImage != null)
                    {
                        normalizedImage.Close();
                        normalizedImage = null;
                    }

                    disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
