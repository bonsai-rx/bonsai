using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Description("Computes the magnitude and angle of the input array of 2D vectors.")]
    public class CartToPolar : ArrayTransform
    {
        [Description("Specifies whether vector angle values are measured in degrees.")]
        public bool AngleInDegrees { get; set; }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var channelFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var x = channelFactory(input, 1);
                var y = channelFactory(input, 1);
                var magnitude = channelFactory(input, 1);
                var angle = channelFactory(input, 1);
                var output = outputFactory(input);
                CV.Split(input, x, y, null, null);
                CV.CartToPolar(x, y, magnitude, angle, AngleInDegrees);
                CV.Merge(magnitude, angle, null, null, output);
                return output;
            });
        }

        public IObservable<Tuple<TArray, TArray>> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
            where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var magnitude = outputFactory(input.Item1);
                var angle = outputFactory(input.Item1);
                CV.CartToPolar(input.Item1, input.Item2, magnitude, angle, AngleInDegrees);
                return Tuple.Create(magnitude, angle);
            });
        }
    }
}
