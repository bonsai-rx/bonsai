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
    [Description("Counts all the non-zero elements in the input array.")]
    public class CountNonZero
    {
        public IObservable<int> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(CV.CountNonZero);
        }
    }
}
