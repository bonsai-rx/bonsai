using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class ConvertToArray
    {
        public IObservable<byte[]> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input =>
            {
                var inputHeader = input.GetMat();
                return ArrHelper.ToArray(inputHeader);
            });
        }
    }
}
