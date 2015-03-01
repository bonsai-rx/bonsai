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
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Merges the input arrays into a single multi-channel array.")]
    public class Merge
    {
        static TArray Process<TArray>(
            TArray item1,
            TArray item2,
            TArray item3,
            TArray item4,
            Func<TArray, int, TArray> outputFactory)
            where TArray : Arr
        {
            var template = item1 ?? item2 ?? item3 ?? item4;
            if (template == null) return null;

            var channels = 0;
            if (item1 != null) channels++;
            if (item2 != null) channels++;
            if (item3 != null) channels++;
            if (item4 != null) channels++;
            var output = outputFactory(template, channels);
            CV.Merge(item1, item2, item3, item4, output);
            return output;
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source) where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            return source.Select(input => Process(input.Item1, input.Item2, null, null, outputFactory));
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray>> source) where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            return source.Select(input => Process(input.Item1, input.Item2, input.Item3, null, outputFactory));
        }

        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            return source.Select(input => Process(input.Item1, input.Item2, input.Item3, input.Item4, outputFactory));
        }
    }
}
