using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Dsp
{
    public class RunningAverage : ArrayTransform
    {
        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double Alpha { get; set; }

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
                        CV.RunningAvg(input, accumulator, Alpha);
                        CV.Convert(accumulator, output);
                        return output;
                    }
                });
            });
        }
    }
}
