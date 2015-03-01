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
    [Description("Calculates the absolute array norm, absolute difference norm, or relative difference norm.")]
    public class Norm
    {
        public Norm()
        {
            NormType = NormTypes.L2;
        }

        [Description("The type of array norm to calculate.")]
        public NormTypes NormType { get; set; }

        public IObservable<double> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => CV.Norm(input, null, NormType));
        }

        public IObservable<double> Process<TArray1, TArray2>(IObservable<Tuple<TArray1, TArray2>> source)
            where TArray1 : Arr
            where TArray2 : Arr
        {
            return source.Select(input => CV.Norm(input.Item1, input.Item2, NormType));
        }

        public IObservable<double> Process<TArray1, TArray2, TMask>(IObservable<Tuple<TArray1, TArray2, TMask>> source)
            where TArray1 : Arr
            where TArray2 : Arr
            where TMask : Arr
        {
            return source.Select(input => CV.Norm(input.Item1, input.Item2, NormType, input.Item3));
        }
    }
}
