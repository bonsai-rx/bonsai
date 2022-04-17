using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that reads the pixel data from each texture in the
    /// sequence to an image.
    /// </summary>
    [Description("Reads the pixel data from each texture in the sequence to an image.")]
    public class RetrieveImage : Transform<Texture, IplImage>
    {
        /// <summary>
        /// Gets or sets the bit depth of each pixel in the retrieved image.
        /// </summary>
        [Description("The bit depth of each pixel in the retrieved image.")]
        public IplDepth Depth { get; set; } = IplDepth.U8;

        /// <summary>
        /// Gets or sets the number of channels in the retrieved image.
        /// </summary>
        [Description("The number of channels in the retrieved image.")]
        public int Channels { get; set; } = 3;

        /// <summary>
        /// Gets or sets a value specifying the flip mode applied to the
        /// retrieved image.
        /// </summary>
        [Description("Specifies the flip mode applied to the retrieved image.")]
        public FlipMode? FlipMode { get; set; }

        /// <summary>
        /// Reads the pixel data from each texture in an observable sequence to
        /// an image.
        /// </summary>
        /// <param name="source">
        /// The sequence of texture objects from which to retrieve the pixel data.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects containing the pixel data
        /// for each texture in the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<Texture> source)
        {
            return Observable.Defer(() =>
            {
                var flipBuffer = default(IplImage);
                return source.Select(texture =>
                {
                    GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                    GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int width);
                    GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int height);
                    var result = new IplImage(new Size(width, height), Depth, Channels);
                    TextureHelper.PackPixelStore(result, out PixelFormat pixelFormat, out PixelType pixelType);
                    GL.GetTexImage(TextureTarget.Texture2D, 0, pixelFormat, pixelType, result.ImageData);

                    var flipMode = FlipMode;
                    if (flipMode.HasValue)
                    {
                        IplImage temp;
                        if (flipBuffer == null) flipBuffer = new IplImage(result.Size, result.Depth, result.Channels);
                        CV.Flip(result, flipBuffer, flipMode.Value);
                        temp = result;
                        result = flipBuffer;
                        flipBuffer = temp;
                    }
                    return result;
                });
            });
        }
    }
}
