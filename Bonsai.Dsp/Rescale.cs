using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that rescales each element in the sequence to a new range
    /// following the specified linear relationship.
    /// </summary>
    [Description("Rescales each element in the sequence to a new range following the specified linear relationship.")]
    public class Rescale : ArrayTransform
    {
        /// <summary>
        /// Gets or sets the lower bound of the range of values in the input sequence.
        /// </summary>
        [Description("The lower bound of the range of values in the input sequence.")]
        public double Min { get; set; } = 0;

        /// <summary>
        /// Gets or sets the upper bound of the range of values in the input sequence.
        /// </summary>
        [Description("The upper bound of the range of values in the input sequence.")]
        public double Max { get; set; } = 1;

        /// <summary>
        /// Gets or sets the lower bound of the range of values after the rescale operation.
        /// </summary>
        [Description("The lower bound of the range of values after the rescale operation.")]
        public double RangeMin { get; set; } = 0;

        /// <summary>
        /// Gets or sets the upper bound of the range of values after the rescale operation.
        /// </summary>
        [Description("The upper bound of the range of values after the rescale operation.")]
        public double RangeMax { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the method used to rescale the values in the
        /// input sequence.
        /// </summary>
        [Description("Specifies the method used to rescale the values in the input sequence.")]
        public RescaleMethod RescaleType { get; set; }

        static void GetScaleShift(double min, double max, double rangeMin, double rangeMax, out double scale, out double shift)
        {
            scale = (rangeMax - rangeMin) / (max - min);
            shift = -min * scale + rangeMin;
        }

        /// <summary>
        /// Rescales every 64-bit floating-point number in an observable sequence to
        /// a new range following the specified linear relationship.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit floating-point numbers.
        /// </param>
        /// <returns>
        /// A sequence of 64-bit floating-point numbers, where each value
        /// has been rescaled following the specified linear relationship.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input =>
            {
                double scale, shift;
                var rangeMin = RangeMin;
                var rangeMax = RangeMax;
                GetScaleShift(Min, Max, rangeMin, rangeMax, out scale, out shift);
                var output = input * scale + shift;
                if (RescaleType == RescaleMethod.Clamp)
                {
                    if (rangeMin > rangeMax)
                    {
                        shift = rangeMin;
                        rangeMin = rangeMax;
                        rangeMax = shift;
                    }
                    output = Math.Max(rangeMin, Math.Min(output, rangeMax));
                }
                return output;
            });
        }

        /// <summary>
        /// Rescales every individual element for all arrays in an observable sequence to
        /// a new range following the specified linear relationship.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each element of the array
        /// has been rescaled following the specified linear relationship.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return source.Select(input =>
            {
                double scale, shift;
                var rangeMin = RangeMin;
                var rangeMax = RangeMax;
                GetScaleShift(Min, Max, rangeMin, rangeMax, out scale, out shift);
                var output = outputFactory(input, Depth.F32);
                CV.ConvertScale(input, output, scale, shift);
                if (RescaleType == RescaleMethod.Clamp)
                {
                    if (rangeMin > rangeMax)
                    {
                        shift = rangeMin;
                        rangeMin = rangeMax;
                        rangeMax = shift;
                    }
                    CV.MinS(output, rangeMax, output);
                    CV.MaxS(output, rangeMin, output);
                }
                return output;
            });
        }
    }
}
