using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class CountNonZero : Transform<IplImage, int>
    {
        public override int Process(IplImage input)
        {
            return Core.cvCountNonZero(input);
        }
    }
}
