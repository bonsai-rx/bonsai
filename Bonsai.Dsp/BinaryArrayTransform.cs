using OpenCV.Net;
using System;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class BinaryArrayTransform
    {
        public abstract IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source) where TArray : Arr;
    }
}
