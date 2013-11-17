using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Transpose : Transform<Mat, Mat>
    {
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var output = new Mat(input.Rows, input.Cols, input.Depth, input.Channels);
                CV.Transpose(input, output);
                return output;
            });
        }
    }
}
