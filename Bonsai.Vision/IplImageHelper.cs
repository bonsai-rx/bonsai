using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Provides helper functions to create and manipulate the format of cached
    /// image buffers.
    /// </summary>
    public static class IplImageHelper
    {
        /// <summary>
        /// Ensures the cached image buffer is allocated and has the specified
        /// size and format parameters.
        /// </summary>
        /// <param name="output">
        /// The current cached image buffer. If the value is <see langword="null"/>,
        /// or if the cached image parameters do not match the specified size
        /// and format, a new image buffer will be allocated.
        /// </param>
        /// <param name="size">The pixel-accurate size of the image.</param>
        /// <param name="depth">The bit depth format for each pixel in the image.</param>
        /// <param name="channels">The number of channels in the image.</param>
        /// <returns>
        /// An <see cref="IplImage"/> object matching the specified size and
        /// format parameters. If <paramref name="output"/> matches all the
        /// parameters, the same reference is returned without modification.
        /// </returns>
        public static IplImage EnsureImageFormat(IplImage output, Size size, IplDepth depth, int channels)
        {
            if (output == null || output.Size != size || output.Depth != depth || output.Channels != channels)
            {
                if (output != null) output.Close();
                return new IplImage(size, depth, channels);
            }

            return output;
        }

        /// <summary>
        /// Copies the original image pixels into a cached image buffer, with optional
        /// color conversion in the case where the original image is grayscale.
        /// </summary>
        /// <param name="output">
        /// The current cached image buffer. If the value is <see langword="null"/>,
        /// or if the cached image parameters do not match the size of the source image,
        /// a new image buffer will be allocated.
        /// </param>
        /// <param name="image">
        /// The image storing the original pixel values.
        /// </param>
        /// <returns>
        /// An <see cref="IplImage"/> object matching the size and bit depth of
        /// <paramref name="image"/> pixels, and where the number of channels is always
        /// three. Pixel values from <paramref name="image"/> will be either copied
        /// or converted from grayscale to BGR, depending on the number of channels.
        /// </returns>
        public static IplImage EnsureColorCopy(IplImage output, IplImage image)
        {
            output = EnsureImageFormat(output, image.Size, image.Depth, 3);
            if (image.Channels == 1) CV.CvtColor(image, output, ColorConversion.Gray2Bgr);
            else CV.Copy(image, output);
            return output;
        }

        static void AdjustRectangle(ref int left, int right, ref int origin, ref int extent)
        {
            if (left < 0)
            {
                origin -= left;
                extent += left;
                left = 0;
            }
            if (right < 0)
            {
                extent += right;
            }
        }

        internal static IplImage CropMakeBorder(
            IplImage image,
            Size size,
            Point? offset,
            IplBorder borderType,
            Scalar fillValue)
        {
            if (size.Width == 0) size.Width = image.Width;
            if (size.Height == 0) size.Height = image.Height;

            Point origin;
            if (offset.HasValue) origin = offset.Value;
            else
            {
                origin.X = (size.Width - image.Width) / 2;
                origin.Y = (size.Height - image.Height) / 2;
            }

            var right = size.Width - origin.X - image.Width;
            var bottom = size.Height - origin.Y - image.Height;
            if (origin.X == 0 && origin.Y == 0 && right == 0 && bottom == 0) return image;

            var inputRect = new Rect(0, 0, image.Width, image.Height);
            AdjustRectangle(ref origin.X, right, ref inputRect.X, ref inputRect.Width);
            AdjustRectangle(ref origin.Y, bottom, ref inputRect.Y, ref inputRect.Height);
            if (origin.X <= 0 && origin.Y <= 0 && right <= 0 && bottom <= 0)
            {
                return image.GetSubRect(inputRect);
            }

            var output = new IplImage(size, image.Depth, image.Channels);
            if (inputRect.Width < 0 || inputRect.Height < 0)
            {
                output.Set(fillValue);
            }
            else
            {
                using var inputHeader = image.GetSubRect(inputRect);
                CV.CopyMakeBorder(inputHeader, output, origin, borderType, fillValue);
            }
            return output;
        }
    }
}
