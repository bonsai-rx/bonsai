using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Sum : Projection<IplImage, CvScalar>
    {
        public override CvScalar Process(IplImage input)
        {
            return Core.cvSum(input);
        }
    }
}
