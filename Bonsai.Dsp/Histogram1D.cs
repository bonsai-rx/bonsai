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
    public class Histogram1D
    {
        public Histogram1D()
        {
            Bins = 10;
            Accumulate = true;
        }

        public float Min { get; set; }

        public float Max { get; set; }

        public int Bins { get; set; }

        public bool Normalize { get; set; }

        public bool Accumulate { get; set; }

        public IObservable<Mat> Process(IObservable<float> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { input })));
        }

        public IObservable<Mat> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return Observable.Defer(() =>
            {
                var histogram = new Histogram(1, new[] { Bins }, HistogramType.Array, new[] { new[] { Min, Max } });
                histogram.Clear();
                return source.Select(input =>
                {
                    histogram.CalcArrHist(new[] { input }, Accumulate);
                    if (Normalize) histogram.Normalize(1);
                    var output = histogram.Bins.GetMat(true).Reshape(0, 1);
                    return output.Clone();
                });
            });
        }
    }
}
