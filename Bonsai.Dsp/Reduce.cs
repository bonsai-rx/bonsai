using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class Reduce : ArrayTransform
    {
        public int Axis { get; set; }

        public ReduceOperation Operation { get; set; }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            return source.Select(input =>
            {
                if (Axis < 0 || Axis > 1)
                {
                    throw new InvalidOperationException("The axis dimension must be either 0 (single row) or 1 (single column).");
                }

                var inputSize = input.Size;
                var outputWidth = Axis == 1 ? 1 : inputSize.Width;
                var outputHeight = Axis == 0 ? 1 : inputSize.Height;
                var outputSize = new Size(outputWidth, outputHeight);
                var output = outputFactory(input, outputSize);
                CV.Reduce(input, output, Axis, Operation);
                return output;
            });
        }
    }
}
