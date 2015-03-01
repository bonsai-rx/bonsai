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
    [Description("Calculates the phase of 2D vectors stored as elements of the input array.")]
    public class Phase : ArrayTransform
    {
        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source) where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input.Item1);
                CV.CartToPolar(input.Item1, input.Item2, null, output);
                return output;
            });
        }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            return Observable.Defer(() =>
            {
                TArray x = null;
                TArray y = null;
                return source.Select(input =>
                {
                    if (x == null)
                    {
                        x = outputFactory(input, 1);
                        y = outputFactory(input, 1);
                    }

                    var output = outputFactory(input, 1);
                    CV.Split(input, x, y, null, null);
                    CV.CartToPolar(x, y, null, output);
                    return output;
                });
            });
        }
    }
}
