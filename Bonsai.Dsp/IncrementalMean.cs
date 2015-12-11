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
    [Description("Incrementally computes the mean of the incoming array sequence.")]
    public class IncrementalMean : ArrayTransform
    {
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            return Observable.Defer(() =>
            {
                var count = 0;
                TArray mean = null;
                var outputFactory = ArrFactory<TArray>.TemplateFactory;
                return source.Select(input =>
                {
                    if (mean == null)
                    {
                        mean = outputFactory(input);
                        mean.SetZero();
                    }

                    var output = outputFactory(input);
                    CV.Sub(input, mean, output);
                    CV.ConvertScale(output, output, 1f / ++count, 0);
                    CV.Add(mean, output, output);
                    mean = output;
                    return output;
                });
            });

        }
    }
}
