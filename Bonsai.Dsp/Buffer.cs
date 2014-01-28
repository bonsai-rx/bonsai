using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class Buffer
    {
        public int Count { get; set; }

        public IObservable<Mat> Process(IObservable<byte> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<short> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<ushort> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<int> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<float> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<double> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }
    }
}
