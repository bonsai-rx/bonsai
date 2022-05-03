using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that reads a block of pixels from the framebuffer.
    /// </summary>
    [Description("Reads a block of pixels from the framebuffer.")]
    public class ReadPixels : Source<IplImage>
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        /// <summary>
        /// Gets or sets the pixel region of interest to read from the framebuffer,
        /// in upper left coordinates. If no region is specified, the entire
        /// framebuffer is read.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The pixel region of interest to read from the framebuffer, in upper left coordinates. If no region is specified, the entire framebuffer is read.")]
        public Rect? RegionOfInterest { get; set; }

        /// <summary>
        /// Generates an observable sequence that reads a block of pixels from
        /// the framebuffer and returns the data as an image object.
        /// </summary>
        /// <returns>
        /// An observable sequence with a single <see cref="IplImage"/> object
        /// storing the pixels read from the framebuffer at the next state update.
        /// </returns>
        public override IObservable<IplImage> Generate()
        {
            return Generate(updateFrame.Generate().Take(1));
        }

        /// <summary>
        /// Reads a block of pixels from the framebuffer whenever an observable
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to read a block of
        /// pixels from the framebuffer.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects storing the pixels
        /// read from the framebuffer whenever the <paramref name="source"/>
        /// sequence emits a notification.
        /// </returns>
        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var flipBuffer = default(IplImage);
                return source.CombineEither(
                    ShaderManager.WindowSource,
                    (input, window) =>
                    {
                        var rect = RegionOfInterest.GetValueOrDefault(new Rect(0, 0, window.Width, window.Height));
                        var result = new IplImage(new Size(rect.Width, rect.Height), IplDepth.U8, 3);
                        rect.Y = window.Height - (rect.Y + rect.Height);
                        TextureHelper.PackPixelStore(result, out PixelFormat pixelFormat, out PixelType pixelType);
                        GL.ReadPixels(rect.X, rect.Y, rect.Width, rect.Height, pixelFormat, pixelType, result.ImageData);
                        if (flipBuffer == null || flipBuffer.Size != result.Size)
                        {
                            flipBuffer = new IplImage(result.Size, result.Depth, result.Channels);
                        }
                        CV.Flip(result, flipBuffer, FlipMode.Vertical);
                        var temp = result;
                        result = flipBuffer;
                        flipBuffer = temp;
                        return result;
                    });
            });
        }
    }
}
