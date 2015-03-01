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
    [Description("Calculates the average (mean) of all the input array elements for each channel.")]
    public class Average
    {
        public IObservable<Scalar> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => CV.Avg(input));
        }

        public IObservable<Scalar> Process<TArray, TMask>(IObservable<Tuple<TArray, TMask>> source)
            where TArray : Arr
            where TMask : Arr
        {
            return source.Select(input => CV.Avg(input.Item1, input.Item2));
        }
    }
}
