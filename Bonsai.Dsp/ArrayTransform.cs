using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class ArrayTransform
    {
        public abstract IObservable<TArray> Process<TArray>(IObservable<TArray> source) where TArray : Arr;
    }
}
