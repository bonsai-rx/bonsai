using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies a forward or inverse linear-polar transform
    /// to each image in the sequence.
    /// </summary>
    /// <remarks>
    /// The transform emulates human foveal image processing.
    /// </remarks>
    [Description("Applies a forward or inverse linear-polar transform to each image in the sequence.")]
    public class LinearPolar : PolarTransform
    {
        /// <summary>
        /// Applies a forward or inverse linear-polar transform to each image in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to apply the linear-polar transform.
        /// </param>
        /// <returns>
        /// The sequence of polar transformed images.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.LinearPolar(input, output, Center, Magnitude, Flags);
                return output;
            });
        }
    }
}
