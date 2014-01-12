using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Transpose : ArrayTransform
    {
        protected override IObservable<TArray> Process<TArray>(IObservable<TArray> source, Func<TArray, TArray> outputFactory)
        {
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Transpose(input, output);
                return output;
            });
        }
    }
}
