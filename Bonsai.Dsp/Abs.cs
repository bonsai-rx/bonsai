using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Calculates the per-element absolute value of input arrays.")]
    public class Abs : ArrayTransform
    {
        public IObservable<sbyte> Process(IObservable<sbyte> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        public IObservable<short> Process(IObservable<short> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Abs(input, output);
                return output;
            });
        }
    }
}
