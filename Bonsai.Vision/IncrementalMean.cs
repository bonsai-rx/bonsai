using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that incrementally computes the mean of the images in the sequence
    /// and returns each intermediate result.
    /// </summary>
    [Obsolete]
    [Description("Incrementally computes the mean of the images in the sequence and returns each intermediate result.")]
    public class IncrementalMean : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Incrementally computes the mean of the images in an observable sequence
        /// and returns each intermediate result.
        /// </summary>
        /// <param name="source">
        /// A sequence of images used to compute the incremental mean.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects, where each image stores the
        /// incremental mean of all previous image values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var count = 0;
                IplImage mean = null;
                return source.Select(input =>
                {
                    if (mean == null)
                    {
                        mean = new IplImage(input.Size, input.Depth, input.Channels);
                        mean.SetZero();
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Sub(input, mean, output);
                    CV.ConvertScale(output, output, 1f / ++count, 0);
                    CV.Add(mean, output, mean);
                    CV.Copy(mean, output);
                    return output;
                });
            });
        }
    }
}
