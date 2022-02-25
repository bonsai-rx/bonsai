using System;
using System.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies an independent scale to the color channels
    /// of every image in the sequence.
    /// </summary>
    [Description("Applies an independent scale to the color channels of every image in the sequence.")]
    public class ColorBalance : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the scale factor applied to every color channel of the image.
        /// </summary>
        [Precision(2, .01)]
        [Range(0, int.MaxValue)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Description("The scale factor applied to every color channel of the input image.")]
        public Scalar Scale { get; set; } = Scalar.All(1);

        /// <summary>
        /// Applies an independent scale to the color channels of every image in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">A sequence of multi-channel images.</param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where every channel has
        /// been multiplied by the corresponding scale factor.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                IplImage channel1 = null;
                IplImage channel2 = null;
                IplImage channel3 = null;
                IplImage channel4 = null;
                return source.Select(input =>
                {
                    channel1 = IplImageHelper.EnsureImageFormat(channel1, input.Size, IplDepth.U8, 1);
                    channel2 = input.Channels > 1 ? IplImageHelper.EnsureImageFormat(channel2, input.Size, IplDepth.U8, 1) : null;
                    channel3 = input.Channels > 2 ? IplImageHelper.EnsureImageFormat(channel3, input.Size, IplDepth.U8, 1) : null;
                    channel4 = input.Channels > 3 ? IplImageHelper.EnsureImageFormat(channel4, input.Size, IplDepth.U8, 1) : null;

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Split(input, channel1, channel2, channel3, channel4);

                    if (channel1 != null) CV.ConvertScale(channel1, channel1, Scale.Val0, 0);
                    if (channel2 != null) CV.ConvertScale(channel2, channel2, Scale.Val1, 0);
                    if (channel3 != null) CV.ConvertScale(channel3, channel3, Scale.Val2, 0);
                    if (channel4 != null) CV.ConvertScale(channel4, channel4, Scale.Val3, 0);
                    CV.Merge(channel1, channel2, channel3, channel4, output);
                    return output;
                });
            });
        }
    }
}
