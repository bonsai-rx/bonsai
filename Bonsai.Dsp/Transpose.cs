using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Transposes every array in the input sequence.")]
    public class Transpose : ArrayTransform
    {
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            return source.Select(input =>
            {
                var inputSize = input.Size;
                var output = outputFactory(input, new Size(inputSize.Height, inputSize.Width));
                CV.Transpose(input, output);
                return output;
            });
        }
    }
}
