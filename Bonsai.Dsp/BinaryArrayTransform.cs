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
    public abstract class BinaryArrayTransform
    {
        public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> source)
        {
            return Process(source, mat => new Mat(mat.Size, mat.Depth, mat.Channels));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return Process(source, image => new IplImage(image.Size, image.Depth, image.Channels));
        }

        protected abstract IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source, Func<TArray, TArray> outputFactory)
            where TArray : Arr;
    }
}
