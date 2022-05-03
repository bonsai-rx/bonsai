using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that generates a sequence with a single image
    /// where all pixels are set to the same color value.
    /// </summary>
    [Description("Generates a sequence with a single image where all pixels are set to the same color value.")]
    public class SolidColor : Source<IplImage>
    {
        /// <summary>
        /// Gets or sets the size of the output image.
        /// </summary>
        [Description("The size of the output image.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the target bit depth of individual image pixels.
        /// </summary>
        [Description("The target bit depth of individual image pixels.")]
        public IplDepth Depth { get; set; } = IplDepth.U8;

        /// <summary>
        /// Gets or sets the number of channels in the output image.
        /// </summary>
        [Description("The number of channels in the output image.")]
        public int Channels { get; set; } = 3;

        /// <summary>
        /// Gets or sets the color value to which all pixels in the output image
        /// will be set to.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color value to which all pixels in the output image will be set to.")]
        public Scalar Color { get; set; }

        IplImage CreateImage()
        {
            var image = new IplImage(Size, Depth, Channels);
            image.Set(Color);
            return image;
        }

        /// <summary>
        /// Generates an observable sequence with a single image where all pixels
        /// are set to the same color value.
        /// </summary>
        /// <returns>
        /// A sequence with a single <see cref="IplImage"/> object with the specified
        /// pixel format and where all pixels are set to the same color value.
        /// </returns>
        public override IObservable<IplImage> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateImage()));
        }

        /// <summary>
        /// Generates an observable sequence of images where all pixels
        /// are set to the same color value, and where each new image is
        /// emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new images.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects with the specified pixel
        /// format and where all pixels are set to the same color value.
        /// </returns>
        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateImage());
        }
    }
}
