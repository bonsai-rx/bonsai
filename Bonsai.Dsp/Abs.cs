using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    public class Abs : ArrayTransform
    {
        protected override IObservable<TArray> Process<TArray>(IObservable<TArray> source, Func<TArray, TArray> outputFactory)
        {
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Abs(input, output);
                return output;
            });
        }
    }
}
