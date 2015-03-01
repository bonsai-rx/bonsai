using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts the input buffer into a managed array of bytes.")]
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
