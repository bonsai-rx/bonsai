using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class CountNonZero : Selector<IplImage, int>
    {
        public override int Process(IplImage input)
        {
            return CV.CountNonZero(input);
        }
    }
}
