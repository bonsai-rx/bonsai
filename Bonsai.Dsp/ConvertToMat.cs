using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts the input array into a multi-channel matrix.")]
    public class ConvertToMat
    {
        public IObservable<Mat> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => input.GetMat());
        }
    }
}
