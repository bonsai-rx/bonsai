using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that segments each HSV image in the sequence using
    /// the specified color range.
    /// </summary>
    [Description("Segments each HSV image in the sequence using the specified color range.")]
    public class HsvThreshold : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the lower bound of the HSV color range.
        /// </summary>
        [TypeConverter(typeof(HsvScalarConverter))]
        [Description("The lower bound of the HSV color range.")]
        public Scalar Lower { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the HSV color range. If the upper value
        /// for Hue is smaller than its lower value, the range will wrap around zero.
        /// </summary>
        [TypeConverter(typeof(HsvScalarConverter))]
        [Description("The upper bound of the HSV color range. If the upper value for Hue is smaller than its lower value, the range will wrap around zero.")]
        public Scalar Upper { get; set; } = new Scalar(179, 255, 255, 255);

        /// <summary>
        /// Segments each HSV image in an observable sequence using the specified
        /// color range.
        /// </summary>
        /// <param name="source">
        /// A sequence of color images in the hue-saturation-value (HSV) color space.
        /// </param>
        /// <returns>
        /// A sequence of binary images where each pixel is non-zero only if the
        /// corresponding HSV pixel in the color image lies between the specified
        /// lower and upper bounds of the range.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var lower = Lower;
                var upper = Upper;
                var output = new IplImage(input.Size, IplDepth.U8, 1);
                if (upper.Val0 < lower.Val0)
                {
                    var upperH = new Scalar(180, upper.Val1, upper.Val2, upper.Val3);
                    var lowerH = new Scalar(0, lower.Val1, lower.Val2, lower.Val3);
                    using (var temp = new IplImage(input.Size, IplDepth.U8, 1))
                    {
                        CV.InRangeS(input, lower, upperH, temp);
                        CV.InRangeS(input, lowerH, upper, output);
                        CV.Or(temp, output, output);
                    }
                }
                else CV.InRangeS(input, lower, upper, output);
                return output;
            });
        }
    }
}
