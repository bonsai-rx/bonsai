using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Dsp
{
    public class Accumulate : ArrayTransform
    {
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            var accumulatorFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return Observable.Defer(() =>
            {
                TArray sum = null;
                return source.Select(input =>
                {
                    var output = outputFactory(input);
                    sum = sum ?? accumulatorFactory(input, Depth.F32);
                    CV.Acc(input, sum);
                    CV.Convert(sum, output);
                    return output;
                });
            });
        }
    }
}
