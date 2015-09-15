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
    [Description("Computes a sequence of two-dimensional histograms from the sequence of input values.")]
    public class Histogram2D
    {
        public Histogram2D()
        {
            BinsX = 10;
            BinsY = 10;
            Accumulate = true;
        }

        [Description("The lower range of the histogram bins in the horizontal dimension.")]
        public float MinX { get; set; }

        [Description("The upper range of the histogram bins in the horizontal dimension.")]
        public float MaxX { get; set; }

        [Description("The lower range of the histogram bins in the vertical dimension.")]
        public float MinY { get; set; }

        [Description("The upper range of the histogram bins in the vertical dimension.")]
        public float MaxY { get; set; }

        [Description("The number of bins in the horizontal dimension of the histogram.")]
        public int BinsX { get; set; }

        [Description("The number of bins in the vertical dimension of the histogram.")]
        public int BinsY { get; set; }

        [Description("Specifies whether the histogram should be normalized such that the sum of bins adds up to one.")]
        public bool Normalize { get; set; }

        [Description("Specifies whether the histogram should be continuously updated.")]
        public bool Accumulate { get; set; }

        Histogram CreateHistogram()
        {
            var histogram = new Histogram(
                2, new[] { BinsY, BinsX },
                HistogramType.Array,
                new[] {
                    new[] { MinY, MaxY },
                    new[] { MinX, MaxX }
                });
            histogram.Clear();
            return histogram;
        }

        public IObservable<IplImage> Process(IObservable<Tuple<int, int>> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { (float)input.Item1, (float)input.Item2 }, 2, 1, Depth.F32, 1)));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<float, float>> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { input.Item1, input.Item2 }, 2, 1, Depth.F32, 1)));
        }

        public IObservable<IplImage> Process(IObservable<Point> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { (float)input.X, (float)input.Y }, 2, 1, Depth.F32, 1)));
        }

        public IObservable<IplImage> Process(IObservable<Point2f> source)
        {
            return Process(source.Select(input => Mat.FromArray(new[] { input.X, input.Y }, 2, 1, Depth.F32, 1)));
        }

        public IObservable<IplImage> Process<TArray>(IObservable<Tuple<TArray, TArray>> source) where TArray : Arr
        {
            return Observable.Defer(() =>
            {
                var histogram = CreateHistogram();
                return source.Select(input =>
                {
                    histogram.CalcArrHist(new[] { input.Item1, input.Item2 }, Accumulate);
                    if (Normalize) histogram.Normalize(1);
                    var output = histogram.Bins.GetMat(true).GetImage();
                    return output.Clone();
                });
            });
        }

        public IObservable<IplImage> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                var histogram = CreateHistogram();
                return source.Select(input =>
                {
                    if (input.Channels == 2)
                    {
                        var ch0 = new Mat(input.Size, input.Depth, 1);
                        var ch1 = new Mat(input.Size, input.Depth, 1);
                        CV.Split(input, ch0, ch1, null, null);
                        histogram.CalcArrHist(new[] { ch1, ch0 }, Accumulate);
                    }
                    else if (input.Rows == 2)
                    {
                        histogram.CalcArrHist(new[] { input.GetRow(1), input.GetRow(0) }, Accumulate);
                    }
                    else throw new InvalidOperationException("The input values must be valid two channel or two row matrices.");

                    if (Normalize) histogram.Normalize(1);
                    var output = histogram.Bins.GetMat(true).Reshape(0, BinsY).GetImage();
                    return output.Clone();
                });
            });
        }
    }
}
