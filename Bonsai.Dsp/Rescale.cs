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
        [Description("The minimum value of the input range.")]
        public double Min { get; set; }

        [Description("The maximum value of the input range.")]
        public double Max { get; set; }

        [Description("The minimum value of the output range.")]
        public double RangeMin { get; set; }

        [Description("The maximum value of the output range.")]
        public double RangeMax { get; set; }

        void GetScaleShift(out double scale, out double shift)
        {
            var min = Min;
            var rangeMin = RangeMin;
            scale = (RangeMax - rangeMin) / (Max - min);
            shift = -min * scale + rangeMin;
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input =>
            {
                double scale, shift;
                GetScaleShift(out scale, out shift);
                return input * scale + shift;
            });
        }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return source.Select(input =>
            {
                double scale, shift;
                GetScaleShift(out scale, out shift);
                var output = outputFactory(input, Depth.F32);
                CV.ConvertScale(input, output, scale, shift);
                return output;
            });
        }
    }
}
