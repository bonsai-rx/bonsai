using System;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the Nth difference between adjacent
    /// samples in the input signal.
    /// </summary>
    [Description("Calculates the Nth difference between adjacent samples in the input signal.")]
    public class Difference : Transform<Mat, Mat>
    {
        int order;
        readonly FirFilter filter = new FirFilter();

        /// <summary>
        /// Initializes a new instance of the <see cref="Difference"/> class.
        /// </summary>
        public Difference()
        {
            Order = 1;
        }

        /// <summary>
        /// Gets or sets the number of times to apply the difference operator.
        /// </summary>
        [Description("The number of times to apply the difference operator.")]
        public int Order
        {
            get { return order; }
            set
            {
                order = value;
                UpdateFilter(order);
            }
        }

        long ComputeBinomialCoefficient(int n, int k)
        {
            // Compute binomial coefficient using multiplicative formula:
            // (n k) = (n*(n-1)*(n-2)*...*(n-(k-1))) / (k*(k-1)*(k-2)*...*1)
            var numerator = 1L;
            for (int i = n; i > n - k; i--)
            {
                numerator *= i;
            }

            var denominator = 1L;
            for (int i = k; i >= 1; i--)
            {
                denominator *= i;
            }

            return numerator / denominator;
        }

        void UpdateFilter(int order)
        {
            var kernel = new float[order + 1];
            for (int k = 0; k <= order; k++)
            {
                var coefficient = ComputeBinomialCoefficient(order, k);
                // Flip the sign of every other coefficient from the anchor since we're subtracting
                if ((order - k) % 2 != 0) coefficient = -coefficient;
                kernel[k] = coefficient;
            }

            filter.Kernel = kernel;
            filter.Anchor = kernel.Length - 1;
        }

        /// <summary>
        /// Calculates the Nth difference between adjacent samples in the input signal.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// signal for which to compute the difference between adjacent samples.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing the differences
        /// between adjacent samples in the input signal.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return filter.Process(source);
        }

        /// <summary>
        /// Calculates the Nth difference between adjacent values in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of floating-point numbers.
        /// </param>
        /// <returns>
        /// A sequence of floating-point numbers representing the differences
        /// between adjacent values in the original sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return filter.Process(source);
        }

        /// <summary>
        /// Calculates the Nth difference between adjacent values in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with single-precision floating-point coordinates.
        /// </param>
        /// <returns>
        /// A sequence of 2D vectors representing the differences between adjacent
        /// points in the original sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<Point2f> source)
        {
            return filter.Process(source);
        }
    }
}
