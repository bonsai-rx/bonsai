using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Computes the accumulated per-element sum of input arrays.")]
    public class Accumulate : ArrayTransform
    {
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            var accumulatorFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return Observable.Defer(() =>
            {
                TArray accumulator = null;
                return source.Select(input =>
                {
                    if (accumulator == null)
                    {
                        accumulator = accumulatorFactory(input, Depth.F32);
                        CV.Convert(input, accumulator);
                        return input;
                    }
                    else
                    {
                        var output = outputFactory(input);
                        CV.Acc(input, accumulator);
                        CV.Convert(accumulator, output);
                        return output;
                    }
                });
            });
        }
    }
}
