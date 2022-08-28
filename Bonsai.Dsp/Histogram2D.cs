using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that computes a sequence of two-dimensional histograms
    /// from each element in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Computes a sequence of two-dimensional histograms from each element in the sequence.")]
    public class Histogram2D
    {
        /// <summary>
        /// Gets or sets the lower range of the histogram bins in the horizontal dimension.
        /// </summary>
        [Description("The lower range of the histogram bins in the horizontal dimension.")]
        public float MinX { get; set; }

        /// <summary>
        /// Gets or sets the upper range of the histogram bins in the horizontal dimension.
        /// </summary>
        [Description("The upper range of the histogram bins in the horizontal dimension.")]
        public float MaxX { get; set; }

        /// <summary>
        /// Gets or sets the lower range of the histogram bins in the vertical dimension.
        /// </summary>
        [Description("The lower range of the histogram bins in the vertical dimension.")]
        public float MinY { get; set; }

        /// <summary>
        /// Gets or sets the upper range of the histogram bins in the vertical dimension.
        /// </summary>
        [Description("The upper range of the histogram bins in the vertical dimension.")]
        public float MaxY { get; set; }

        /// <summary>
        /// Gets or sets the number of bins in the horizontal dimension of the histogram.
        /// </summary>
        [Description("The number of bins in the horizontal dimension of the histogram.")]
        public int BinsX { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of bins in the vertical dimension of the histogram.
        /// </summary>
        [Description("The number of bins in the vertical dimension of the histogram.")]
        public int BinsY { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value specifying whether the histogram should be normalized
        /// such that the sum of bins adds up to one.
        /// </summary>
        [Description("Specifies whether the histogram should be normalized such that the sum of bins adds up to one.")]
        public bool Normalize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the histogram should be continuously updated.
        /// </summary>
        [Description("Specifies whether the histogram should be continuously updated.")]
        public bool Accumulate { get; set; } = true;

        Histogram CreateHistogram()
        {
            var histogram = new Histogram(
                2, new[] { BinsY, BinsX },
                HistogramType.Array,
                new[] {
                    new[] { MinY, MaxY },
                    new[] { MinX, MaxX }
                });
            histogram.Clear();
            return histogram;
        }

        /// <summary>
        /// Computes a sequence of two-dimensional histograms from an observable sequence
        /// of pairs of integer coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs of integer coordinates used to calculate the two-dimensional
        /// histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the two-dimensional
        /// histogram calculated from the values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<int, int>> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { (float)input.Item1, (float)input.Item2 }, 2, 1, Depth.F32, 1)));
        }

        /// <summary>
        /// Computes a sequence of two-dimensional histograms from an observable sequence
        /// of pairs of single-precision floating-point coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs of single-precision floating-point coordinates used
        /// to calculate the two-dimensional histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the two-dimensional
        /// histogram calculated from the values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<float, float>> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { input.Item1, input.Item2 }, 2, 1, Depth.F32, 1)));
        }

        /// <summary>
        /// Computes a sequence of two-dimensional histograms from an observable sequence
        /// of 2D points with integer coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with integer coordinates used to calculate the
        /// two-dimensional histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the two-dimensional
        /// histogram calculated from the values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Point> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { (float)input.X, (float)input.Y }, 2, 1, Depth.F32, 1)));
        }

        /// <summary>
        /// Computes a sequence of two-dimensional histograms from an observable sequence
        /// of 2D points with single-precision floating-point coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with single-precision floating-point coordinates used
        /// to calculate the two-dimensional histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the two-dimensional
        /// histogram calculated from the values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Point2f> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { input.X, input.Y }, 2, 1, Depth.F32, 1)));
        }

        /// <summary>
        /// Computes a sequence of two-dimensional histograms from an observable sequence
        /// of pairs of one-dimensional arrays of coordinates.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of one-dimensional arrays, where each array represents
        /// respectively the horizontal and vertical dimensions used to calculate the
        /// two-dimensional histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the two-dimensional
        /// histogram calculated from the values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<IplImage> Process<TArray>(IObservable<Tuple<TArray, TArray>> source) where TArray : Arr
        {
            return Observable.Defer(() =>
            {
                var histogram = CreateHistogram();
                return source.Select(input =>
                {
                    histogram.CalcArrHist(new[] { input.Item1, input.Item2 }, Accumulate);
                    if (Normalize) histogram.Normalize(1);
                    var output = histogram.Bins.GetMat(true).GetImage();
                    return output.Clone();
                });
            });
        }

        /// <summary>
        /// Computes a sequence of two-dimensional histograms from an observable sequence
        /// of multi-channel arrays of point coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of two-channel arrays, or single-channel arrays with two rows, where
        /// the different elements in an array represent the horizontal and vertical dimensions
        /// used to calculate the two-dimensional histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the two-dimensional
        /// histogram calculated from the values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                var histogram = CreateHistogram();
                return source.Select(input =>
                {
                    if (input.Channels == 2)
                    {
                        var ch0 = new Mat(input.Size, input.Depth, 1);
                        var ch1 = new Mat(input.Size, input.Depth, 1);
                        CV.Split(input, ch0, ch1, null, null);
                        histogram.CalcArrHist(new[] { ch1, ch0 }, Accumulate);
                    }
                    else if (input.Rows == 2)
                    {
                        histogram.CalcArrHist(new[] { input.GetRow(1), input.GetRow(0) }, Accumulate);
                    }
                    else throw new InvalidOperationException("The input values must be valid two channel or two row matrices.");

                    if (Normalize) histogram.Normalize(1);
                    var output = histogram.Bins.GetMat(true).Reshape(0, BinsY).GetImage();
                    return output.Clone();
                });
            });
        }
    }
}
