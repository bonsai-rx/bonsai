using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that tests which pixels lie within the specified
    /// range for each image in the sequence.
    /// </summary>
    [Description("Tests which pixels lie within the specified range for each image in the sequence.")]
    public class RangeThreshold : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the inclusive lower boundary of the range.
        /// </summary>
        [TypeConverter(typeof(RangeScalarConverter))]
        [Description("The inclusive lower boundary of the range.")]
        public Scalar Lower { get; set; }

        /// <summary>
        /// Gets or sets the exclusive upper boundary of the range.
        /// </summary>
        [TypeConverter(typeof(RangeScalarConverter))]
        [Description("The exclusive upper boundary of the range.")]
        public Scalar Upper { get; set; } = new Scalar(255, 255, 255, 255);

        /// <summary>
        /// Tests which pixels lie within the specified range for each image in
        /// an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to threshold. Each channel in a color image
        /// is tested independently according to the specified scalar range.
        /// </param>
        /// <returns>
        /// A sequence of binary images where each pixel is non-zero if the
        /// corresponding value in the original image is within the allowable range.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.U8, 1);
                CV.InRangeS(input, Lower, Upper, output);
                return output;
            });
        }
    }
}
