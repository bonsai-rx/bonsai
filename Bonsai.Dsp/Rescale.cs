using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Description("Rescales the elements of the input sequence to a new range.")]
    public class Rescale : ArrayTransform
    {
        public Rescale()
        {
            Min = 0;
            Max = 1;
            RangeMin = 0;
            RangeMax = 1;
        }

        [Description("The minimum value of the input range.")]
        public double Min { get; set; }

        [Description("The maximum value of the input range.")]
        public double Max { get; set; }

        [Description("The minimum value of the output range.")]
        public double RangeMin { get; set; }

        [Description("The maximum value of the output range.")]
        public double RangeMax { get; set; }

        [Description("The method used to rescale the input range.")]
        public RescaleMethod RescaleType { get; set; }

        static void GetScaleShift(double min, double max, double rangeMin, double rangeMax, out double scale, out double shift)
        {
            scale = (rangeMax - rangeMin) / (max - min);
            shift = -min * scale + rangeMin;
        }

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
