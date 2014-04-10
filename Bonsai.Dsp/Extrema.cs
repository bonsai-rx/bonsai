using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Finds the global minimum and maximum of all the array elements.")]
    public class Extrema
    {
        static ArrayExtrema ProcessExtrema(Arr arr, Arr mask = null)
        {
            var extrema = new ArrayExtrema();
            CV.MinMaxLoc(arr,
                         out extrema.MinValue,
                         out extrema.MaxValue,
                         out extrema.MinLocation,
                         out extrema.MaxLocation,
                         mask);
            return extrema;
        }

        public IObservable<ArrayExtrema> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => ProcessExtrema(input));
        }

        public IObservable<ArrayExtrema> Process<TArray, TMask>(IObservable<Tuple<TArray, TMask>> source)
            where TArray : Arr
            where TMask : Arr
        {
            return source.Select(input => ProcessExtrema(input.Item1, input.Item2));
        }
    }
}
