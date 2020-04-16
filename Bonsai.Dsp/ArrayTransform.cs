using OpenCV.Net;
using System;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class ArrayTransform
    {
        public abstract IObservable<TArray> Process<TArray>(IObservable<TArray> source) where TArray : Arr;
    }
}
