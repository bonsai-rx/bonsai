using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Transpose : Selector<Mat, Mat>
    {
        public override Mat Process(Mat input)
        {
            var output = new Mat(input.Rows, input.Cols, input.Depth, input.Channels);
            CV.Transpose(input, output);
            return output;
        }
    }
}
