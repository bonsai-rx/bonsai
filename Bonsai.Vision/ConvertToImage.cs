using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts the input array into an image.")]
    public class ConvertToImage
    {
        public IObservable<IplImage> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => input.GetImage());
        }
    }
}
