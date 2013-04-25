using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class CountNonZero : Transform<CvMat, int>
    {
        public override int Process(CvMat input)
        {
            return Core.cvCountNonZero(input);
        }
    }
}
