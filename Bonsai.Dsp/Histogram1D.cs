using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that computes a sequence of one-dimensional histograms
    /// from each element in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Computes a sequence of one-dimensional histograms from each element in the sequence.")]
    public class Histogram1D
    {
        /// <summary>
        /// Gets or sets the lower range of the histogram bins.
        /// </summary>
        [Description("The lower range of the histogram bins.")]
        public float Min { get; set; }

        /// <summary>
        /// Gets or sets the upper range of the histogram bins.
        /// </summary>
        [Description("The upper range of the histogram bins.")]
        public float Max { get; set; }

        /// <summary>
        /// Gets or sets the number of bins in the histogram.
        /// </summary>
        [Description("The number of bins in the histogram.")]
        public int Bins { get; set; } = 10;

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

        /// <summary>
        /// Computes an observable sequence of one-dimensional histograms from each
        /// element in the source sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit floating-point numbers used to calculate the
        /// one-dimensional histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing the one-dimensional
        /// histogram calculated from the values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Mat> Process(IObservable<float> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { input })));
        }

        /// <summary>
        /// Computes an observable sequence of one-dimensional histograms from each
        /// element in the source sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of array values used to calculate the one-dimensional histogram.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing the one-dimensional
        /// histogram calculated from the array values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Mat> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return Observable.Defer(() =>
            {
                var histogram = new Histogram(1, new[] { Bins }, HistogramType.Array, new[] { new[] { Min, Max } });
                histogram.Clear();
                return source.Select(input =>
                {
                    histogram.CalcArrHist(new[] { input }, Accumulate);
                    if (Normalize) histogram.Normalize(1);
                    var output = histogram.Bins.GetMat(true).Reshape(0, 1);
                    return output.Clone();
                });
            });
        }
    }
}
