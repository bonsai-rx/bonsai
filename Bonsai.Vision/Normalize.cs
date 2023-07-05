using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that normalizes the range of values for each image
    /// in the sequence to be between zero and one.
    /// </summary>
    [Description("Normalizes the range of values for each image in the sequence to be between zero and one.")]
    public class Normalize : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Normalizes the range of values for each image in an observable sequence
        /// to be between zero and one.
        /// </summary>
        /// <param name="source">The sequence of images to normalize.</param>
        /// <returns>
        /// The sequence of normalized images.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.F32, input.Channels);
                CV.MinMaxLoc(input, out double min, out double max, out Point minLoc, out Point maxLoc);

                var range = max - min;
                var scale = range > 0 ? 1.0 / range : 0;
                var shift = range > 0 ? -min * scale : 0;
                CV.ConvertScale(input, output, scale, shift);
                return output;
            });
        }
    }
}
