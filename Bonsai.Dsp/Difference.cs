using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    [Description("Calculates the Nth difference between adjacent elements of the input array.")]
    public class Difference : Transform<Mat, Mat>
    {
        int order;
        FirFilter filter = new FirFilter();

        public Difference()
        {
            Order = 1;
        }

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

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return filter.Process(source);
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return filter.Process(source);
        }

        public IObservable<Point2f> Process(IObservable<Point2f> source)
        {
            return filter.Process(source);
        }
    }
}
