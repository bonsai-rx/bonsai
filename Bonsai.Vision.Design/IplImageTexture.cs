using System;
using System.Linq;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Represents a texture buffer which can be updated with <see cref="IplImage"/>
    /// data and rendered onto the current viewport as a full-screen quad.
    /// </summary>
    public class IplImageTexture : IDisposable
    {
        bool disposed;
        uint vbo;
        int texture;
        readonly int maxTextureSize;
        readonly bool nonPowerOfTwo;
        IplImage textureImage;
        IplImage normalizedImage;
        Size textureSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageTexture"/> class.
        /// </summary>
        public IplImageTexture()
        {
            var extensions = GL.GetString(StringName.Extensions).Split(' ');
            nonPowerOfTwo = extensions.Contains("GL_ARB_texture_non_power_of_two");
            GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);

            GL.GenBuffers(1, out vbo);
            var vertices = new float[]
            {
                0f, 1f, -1f, -1f,
                1f, 1f,  1f, -1f,
                1f, 0f,  1f,  1f,
                0f, 0f, -1f,  1f,
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
            GL.VertexPointer(2, VertexPointerType.Float, 4 * sizeof(float), 2 * sizeof(float));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

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

        /// <summary>
        /// Updates the texture buffer with the specified image data.
        /// </summary>
        /// <param name="image">
        /// An <see cref="IplImage"/> object containing the pixel data to copy
        /// into the texture buffer.
        /// </param>
        public void Update(IplImage image)
        {
            Update(image, 1.0);
        }

        internal void Update(IplImage image, double scale)
        {
            var shift = 0.0;
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (image.Depth != IplDepth.U8)
            {
                double min, max;
                using (var buffer = image.Reshape(1, 0))
                {
                    CV.MinMaxLoc(buffer, out min, out max, out _, out _);
                }

                var range = scale * (max - min);
                scale = range > 0 ? 255.0 / range : 0;
                shift = range > 0 ? -min : 0;
            }

            if (scale != 1.0 || image.Depth != IplDepth.U8)
            {
                normalizedImage = IplImageHelper.EnsureImageFormat(normalizedImage, image.Size, IplDepth.U8, image.Channels);
                CV.ConvertScale(image, normalizedImage, scale, shift * scale);
                image = normalizedImage;
            }

            if (!nonPowerOfTwo || image.Width > maxTextureSize || image.Height > maxTextureSize)
            {
                var textureWidth = Math.Min(maxTextureSize, NearestPowerOfTwo(image.Width));
                var textureHeight = Math.Min(maxTextureSize, NearestPowerOfTwo(image.Height));
                textureImage = IplImageHelper.EnsureImageFormat(textureImage, new Size(textureWidth, textureHeight), image.Depth, image.Channels);
                CV.Resize(image, textureImage, SubPixelInterpolation.Linear);
                image = textureImage;
            }

            PixelFormat pixelFormat;
            switch (image.Channels)
            {
                case 1: pixelFormat = PixelFormat.Luminance; break;
                case 2: pixelFormat = PixelFormat.Rg; break;
                case 3: pixelFormat = PixelFormat.Bgr; break;
                case 4: pixelFormat = PixelFormat.Bgra; break;
                default: throw new ArgumentException("Image has an unsupported number of channels.", nameof(image));
            }

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, image.WidthStep % 4 == 0 ? 4 : 1);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, image.WidthStep / image.Channels);
            if (textureSize != image.Size)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, pixelFormat, PixelType.UnsignedByte, image.ImageData);
                textureSize = image.Size;
            }
            else GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, image.Width, image.Height, pixelFormat, PixelType.UnsignedByte, image.ImageData); 
        }

        /// <summary>
        /// Binds and draws the texture buffer object as a full-screen quad.
        /// </summary>
        public void Draw()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GL.DeleteTextures(1, ref texture);
                    GL.DeleteBuffers(1, ref vbo);
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

        /// <summary>
        /// Releases all resources used by the <see cref="IplImageTexture"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
