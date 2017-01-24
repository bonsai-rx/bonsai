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
    [Description("Calculates the exponent of every input array element.")]
    public class Exp : ArrayTransform
    {
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input => Math.Exp(input));
        }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Exp(input, output);
                return output;
            });
        }
    }
}
