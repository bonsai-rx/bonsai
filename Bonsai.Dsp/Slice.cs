using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that splits each array in the sequence into multiple
    /// sub-arrays along the specified dimension.
    /// </summary>
    [Combinator]
    [Description("Splits each array in the sequence into multiple sub-arrays along the specified dimension.")]
    public class Slice
    {
        /// <summary>
        /// Gets or sets the dimension along which to slice the array.
        /// </summary>
        [Description("The dimension along which to slice the array.")]
        public int Axis { get; set; }

        /// <summary>
        /// Gets or sets the number of elements in each slice.
        /// </summary>
        [Description("The number of elements in each slice.")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between the creation of each slice.
        /// If it is not specified, it will be set to the number of elements in each slice.
        /// </summary>
        [Description("The optional number of elements to skip between the creation of each slice.")]
        public int? Skip { get; set; }

        static IEnumerable<IplImage> SliceRows(IplImage input, int count, int skip)
        {
            for (int i = 0; i <= input.Height - skip; i += skip)
            {
                var rectangle = new Rect(0, i, input.Width, count);
                yield return input.GetSubRect(rectangle);
            }
        }

        static IEnumerable<IplImage> SliceCols(IplImage input, int count, int skip)
        {
            for (int i = 0; i <= input.Width - skip; i += skip)
            {
                var rectangle = new Rect(i, 0, count, input.Height);
                yield return input.GetSubRect(rectangle);
            }
        }

        static IEnumerable<Mat> SliceRows(Mat input, int count, int skip)
        {
            for (int i = 0; i <= input.Rows - skip; i += skip)
            {
                var rectangle = new Rect(0, i, input.Cols, count);
                yield return input.GetSubRect(rectangle);
            }
        }

        static IEnumerable<Mat> SliceCols(Mat input, int count, int skip)
        {
            for (int i = 0; i <= input.Cols - skip; i += skip)
            {
                var rectangle = new Rect(i, 0, count, input.Rows);
                yield return input.GetSubRect(rectangle);
            }
        }

        /// <summary>
        /// Splits each image in an observable sequence into multiple
        /// sub-images along the specified dimension.
        /// </summary>
        /// <param name="source">
        /// A sequence of image values.
        /// </param>
        /// <returns>
        /// A sequence of image values, where each image represents a slice of
        /// the original image, along the specified direction, with the specified
        /// number of elements.
        /// </returns>
        /// <remarks>
        /// If <see cref="Count"/> is smaller than the size of the images, this
        /// operator will return multiple sub-images for each image in the
        /// <paramref name="source"/> sequence.
        /// </remarks>
        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.SelectMany(input =>
            {
                var axis = Axis;
                var count = Count;
                var skip = Skip.GetValueOrDefault(count);
                return axis == 0 ? SliceRows(input, count, skip) : SliceCols(input, count, skip);
            });
        }

        /// <summary>
        /// Splits each matrix in an observable sequence into multiple
        /// sub-matrices along the specified dimension.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D matrix values.
        /// </param>
        /// <returns>
        /// A sequence of 2D matrix values, where each matrix represents a slice of
        /// the original matrix, along the specified direction, with the specified
        /// number of elements.
        /// </returns>
        /// <remarks>
        /// If <see cref="Count"/> is smaller than the size of the matrices, this
        /// operator will return multiple sub-matrices for each matrix in the
        /// <paramref name="source"/> sequence.
        /// </remarks>
        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.SelectMany(input =>
            {
                var axis = Axis;
                var count = Count;
                var skip = Skip.GetValueOrDefault(count);
                return axis == 0 ? SliceRows(input, count, skip) : SliceCols(input, count, skip);
            });
        }
    }
}
