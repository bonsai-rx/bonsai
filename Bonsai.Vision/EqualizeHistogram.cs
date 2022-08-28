using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that equalizes the histogram of every grayscale
    /// image in the sequence.
    /// </summary>
    [Description("Equalizes the histogram of every grayscale image in the sequence.")]
    public class EqualizeHistogram : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Equalizes the histogram of every grayscale image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of grayscale images for which to equalize the histogram.
        /// </param>
        /// <returns>
        /// A sequence of images representing the original image with an equalized
        /// pixel brightness histogram.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.EqualizeHist(input, output);
                return output;
            });
        }
    }
}
