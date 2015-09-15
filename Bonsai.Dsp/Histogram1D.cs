using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Computes a sequence of one-dimensional histograms from the sequence of input values.")]
    public class Histogram1D
    {
        public Histogram1D()
        {
            Bins = 10;
            Accumulate = true;
        }

        [Description("The lower range of the histogram bins.")]
        public float Min { get; set; }

        [Description("The upper range of the histogram bins.")]
        public float Max { get; set; }

        [Description("The number of bins in the histogram.")]
        public int Bins { get; set; }

        [Description("Specifies whether the histogram should be normalized such that the sum of bins adds up to one.")]
        public bool Normalize { get; set; }

        [Description("Specifies whether the histogram should be continuously updated.")]
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
