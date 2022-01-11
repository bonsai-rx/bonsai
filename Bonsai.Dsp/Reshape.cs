using OpenCV.Net;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that changes the shape of each array in the sequence
    /// without copying data.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Changes the shape of each array in the sequence without copying data.")]
    public class Reshape
    {
        /// <summary>
        /// Gets or sets the new number of channels. Zero means the number of channels will not change.
        /// </summary>
        [Description("The new number of channels. Zero means the number of channels will not change.")]
        public int Channels { get; set; }

        /// <summary>
        /// Gets or sets the new number of rows. Zero means the number of rows will not change.
        /// </summary>
        [Description("The new number of rows. Zero means the number of rows will not change.")]
        public int Rows { get; set; }

        /// <summary>
        /// Changes the shape of each matrix in an observable sequence without copying data.
        /// </summary>
        /// <param name="source">
        /// The sequence of multi-channel matrices to be reshaped.
        /// </param>
        /// <returns>
        /// The sequence of reshaped multi-channel matrices.
        /// </returns>
        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input => input.Reshape(Channels, Rows));
        }

        /// <summary>
        /// Changes the shape of each image in an observable sequence without copying data.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to be reshaped.
        /// </param>
        /// <returns>
        /// The sequence of reshaped images.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input => input.Reshape(Channels, Rows).GetImage());
        }
    }
}
