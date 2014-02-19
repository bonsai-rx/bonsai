using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class Concat : BinaryArrayTransform
    {
        public int Axis { get; set; }

        public override IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            return source.Select(input =>
            {
                var axis = Axis;
                var first = input.Item1;
                var second = input.Item2;
                var firstSize = first.Size;
                var secondSize = second.Size;
                if (first.ElementType != second.ElementType)
                {
                    throw new InvalidOperationException("Input arrays must have the same element type.");
                }

                if (axis == 0 && firstSize.Width != secondSize.Width ||
                    axis == 1 && firstSize.Height != secondSize.Height)
                {
                    throw new InvalidOperationException("Input arrays must have the same shape except in the dimension corresponding to axis.");
                }

                var rowOffset = axis == 0 ? firstSize.Height : 0;
                var columnOffset = axis == 1 ? firstSize.Width : 0;
                var outputWidth = axis == 0 ? firstSize.Width : firstSize.Width + secondSize.Width;
                var outputHeight = axis == 1 ? firstSize.Height : firstSize.Height + secondSize.Height;
                var output = outputFactory(first, new Size(outputWidth, outputHeight));
                using (var firstOutput = output.GetSubRect(new Rect(0, 0, firstSize.Width, firstSize.Height)))
                using (var secondOutput = output.GetSubRect(new Rect(columnOffset, rowOffset, secondSize.Width, secondSize.Height)))
                {
                    CV.Copy(first, firstOutput);
                    CV.Copy(second, secondOutput);
                }
                return output;
            });
        }
    }
}
