using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Calculates the absolute difference between the two input arrays.")]
    public class AbsoluteDifference : BinaryArrayTransform
    {
        public override IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var first = input.Item1;
                var second = input.Item2;
                var output = outputFactory(first);
                CV.AbsDiff(first, second, output);
                return output;
            });
        }
    }
}
