using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that converts pixels from one color space to another
    /// for all images in the sequence.
    /// </summary>
    [Description("Converts pixels from one color space to another for all images in the sequence.")]
    public class ConvertColor : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the color conversion to apply to individual image pixels.
        /// </summary>
        [Description("The color conversion to apply to individual image pixels.")]
        public ColorConversion Conversion { get; set; } = ColorConversion.Bgr2Hsv;

        /// <summary>
        /// Converts pixels from one color space to another for all images in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images in the original color space.
        /// </param>
        /// <returns>
        /// A sequence of images where every pixel is specified in the new color space.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var conversion = Conversion;
                var numChannels = conversion.GetConversionNumChannels();
                return source.Select(input =>
                {
                    if (conversion != Conversion)
                    {
                        conversion = Conversion;
                        numChannels = conversion.GetConversionNumChannels();
                    }

                    var output = new IplImage(input.Size, input.Depth, numChannels);
                    CV.CvtColor(input, output, conversion);
                    return output;
                });
            });
        }
    }
}
