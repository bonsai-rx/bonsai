using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Subtract : Transform<Tuple<Mat, Mat>, Mat>
    {
        public override IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> source)
        {
            return source.Select(input =>
            {
                var first = input.Item1;
                var second = input.Item2;
                var output = new Mat(first.Rows, first.Cols, first.Depth, first.Channels);
                CV.Sub(first, second, output);
                return output;
            });
        }
    }
}
