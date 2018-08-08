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
    [Description("Concatenates each set of input arrays into a single buffer.")]
    public class Concat : BinaryArrayTransform
    {
        [Description("The dimension along which to merge the arrays.")]
        public int Axis { get; set; }

        TElement[] Process<TElement>(IEnumerable<TElement[]> sources)
        {
            var length = 0;
            foreach (var source in sources)
            {
                length += source.Length;
            }

            var offset = 0;
            var output = new TElement[length];
            foreach (var source in sources)
            {
                Array.Copy(source, 0, output, offset, source.Length);
                offset += source.Length;
            }
            return output;
        }

        TArray Process<TArray>(IEnumerable<TArray> sources) where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            var axis = Axis;

            TArray first = null;
            int elementType = 0;
            int elementWidth = 0;
            int elementHeight = 0;
            int outputWidth = 0;
            int outputHeight = 0;
            foreach (var source in sources)
            {
                var size = source.Size;
                if (first == null)
                {
                    first = source;
                    elementType = first.ElementType;
                    elementWidth = size.Width;
                    elementHeight = size.Height;
                    outputWidth = axis == 0 ? size.Width : 0;
                    outputHeight = axis == 1 ? size.Height : 0;
                }

                if (source.ElementType != elementType)
                {
                    throw new InvalidOperationException("Input arrays must have the same element type.");
                }

                if (axis == 0 && size.Width != elementWidth ||
                    axis == 1 && size.Height != elementHeight)
                {
                    throw new InvalidOperationException("Input arrays must have the same shape except in the dimension corresponding to axis.");
                }

                outputWidth += axis == 1 ? size.Width : 0;
                outputHeight += axis == 0 ? size.Height : 0;
            }

            int rowOffset = 0;
            int columnOffset = 0;
            var output = outputFactory(first, new Size(outputWidth, outputHeight));
            foreach (var source in sources)
            {
                var size = source.Size;
                using (var subRect = output.GetSubRect(new Rect(columnOffset, rowOffset, size.Width, size.Height)))
                {
                    CV.Copy(source, subRect);
                }

                rowOffset += axis == 0 ? size.Height : 0;
                columnOffset += axis == 1 ? size.Width : 0;
            }

            return output;
        }

        public override IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2 }));
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3 }));
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4 }));
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5 }));
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6 }));
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6, input.Item7 }));
        }

        public IObservable<TArray> Process<TArray>(IObservable<IList<TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(input));
        }

        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2 }));
        }

        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3 }));
        }

        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4 }));
        }

        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5 }));
        }

        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6 }));
        }

        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6, input.Item7 }));
        }

        public IObservable<TElement[]> Process<TElement>(IObservable<IList<TElement[]>> source)
        {
            return source.Select(input => Process(input));
        }
    }
}
