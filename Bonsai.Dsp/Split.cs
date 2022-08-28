using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that splits the channels of each array in the sequence into separate arrays.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Splits the channels of each array in the sequence into separate arrays.")]
    public class Split
    {
        /// <summary>
        /// Splits the channels of each matrix in an observable sequence into
        /// separate matrices.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D matrix values.
        /// </param>
        /// <returns>
        /// A sequence of tuples of 2D matrix values, where each matrix represents
        /// a different channel from the original matrix. If the matrix has less than
        /// four channels, the remaining elements in the tuple after the last channel
        /// will be set to <see langword="null"/>.
        /// </returns>
        public IObservable<Tuple<Mat, Mat, Mat, Mat>> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var c0 = input.Channels > 1 ? new Mat(input.Size, input.Depth, 1) : input;
                var c1 = input.Channels > 1 ? new Mat(input.Size, input.Depth, 1) : null;
                var c2 = input.Channels > 2 ? new Mat(input.Size, input.Depth, 1) : null;
                var c3 = input.Channels > 3 ? new Mat(input.Size, input.Depth, 1) : null;
                if (input.Channels > 1)
                {
                    CV.Split(input, c0, c1, c2, c3);
                }

                return Tuple.Create(c0, c1, c2, c3);
            });
        }

        /// <summary>
        /// Splits the channels of each image in an observable sequence into
        /// separate images.
        /// </summary>
        /// <param name="source">
        /// A sequence of image values.
        /// </param>
        /// <returns>
        /// A sequence of tuples of image values, where each image represents
        /// a different channel from the original matrix. If the image has less than
        /// four channels, the remaining elements in the tuple after the last channel
        /// will be set to <see langword="null"/>.
        /// </returns>
        public IObservable<Tuple<IplImage, IplImage, IplImage, IplImage>> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var c0 = input.Channels > 1 ? new IplImage(input.Size, input.Depth, 1) : input;
                var c1 = input.Channels > 1 ? new IplImage(input.Size, input.Depth, 1) : null;
                var c2 = input.Channels > 2 ? new IplImage(input.Size, input.Depth, 1) : null;
                var c3 = input.Channels > 3 ? new IplImage(input.Size, input.Depth, 1) : null;
                if (input.Channels > 1)
                {
                    CV.Split(input, c0, c1, c2, c3);
                }

                return Tuple.Create(c0, c1, c2, c3);
            });
        }
    }
}
