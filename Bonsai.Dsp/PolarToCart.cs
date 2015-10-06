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
    [Description("Computes the cartesian coordinates of 2D vectors represented in polar form.")]
    public class PolarToCart : ArrayTransform
    {
        [Description("Specifies whether vector angle values are measured in degrees.")]
        public bool AngleInDegrees { get; set; }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var channelFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var magnitude = channelFactory(input, 1);
                var angle = channelFactory(input, 1);
                var x = channelFactory(input, 1);
                var y = channelFactory(input, 1);
                var output = outputFactory(input);
                CV.Split(input, magnitude, angle, null, null);
                CV.PolarToCart(magnitude, angle, x, y, AngleInDegrees);
                CV.Merge(x, y, null, null, output);
                return output;
            });
        }

        public IObservable<Tuple<TArray, TArray>> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
            where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var x = outputFactory(input.Item1);
                var y = outputFactory(input.Item1);
                CV.PolarToCart(input.Item1, input.Item2, x, y, AngleInDegrees);
                return Tuple.Create(x, y);
            });
        }
    }
}
