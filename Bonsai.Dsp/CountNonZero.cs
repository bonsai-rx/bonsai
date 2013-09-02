using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class CountNonZero : Selector<Mat, int>
    {
        public override int Process(Mat input)
        {
            return CV.CountNonZero(input);
        }
    }
}
