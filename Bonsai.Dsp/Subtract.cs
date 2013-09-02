using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Subtract : Selector<Mat, Mat, Mat>
    {
        public override Mat Process(Mat first, Mat second)
        {
            var output = new Mat(first.Rows, first.Cols, first.Depth, first.Channels);
            CV.Sub(first, second, output);
            return output;
        }
    }
}
