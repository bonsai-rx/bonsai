using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that converts each BGR color image in the
    /// sequence to grayscale.
    /// </summary>
    [Description("Converts each BGR color image in the sequence to grayscale.")]
    public class Grayscale : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Converts each BGR color image in an observable sequence to grayscale.
        /// </summary>
        /// <param name="source">
        /// A sequence of color images in the blue-green-red (BGR) color space.
        /// </param>
        /// <returns>
        /// A sequence of grayscale single-channel images.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                if (input.Channels == 1)
                {
                    return input;
                }
                else
                {
                    var output = new IplImage(input.Size, input.Depth, 1);
                    CV.CvtColor(input, output, ColorConversion.Bgr2Gray);
                    return output;
                }
            });
        }
    }
}
