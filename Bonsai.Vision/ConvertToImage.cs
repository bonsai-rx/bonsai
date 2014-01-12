using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class ConvertToImage
    {
        public IObservable<IplImage> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => input.GetImage());
        }
    }
}
