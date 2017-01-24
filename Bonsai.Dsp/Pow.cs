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
    [Description("Raises every input array element to a power.")]
    public class Pow : ArrayTransform
    {
        [Description("Specifies the power exponent.")]
        public double Power { get; set; }

        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input => Math.Pow(input, Power));
        }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Pow(input, output, Power);
                return output;
            });
        }
    }
}
