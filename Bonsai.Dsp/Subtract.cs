using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Subtract : Transform<CvMat, CvMat, CvMat>
    {
        public override CvMat Process(CvMat first, CvMat second)
        {
            var output = new CvMat(first.Rows, first.Cols, first.Depth, first.NumChannels);
            Core.cvSub(first, second, output, CvArr.Null);
            return output;
        }
    }
}
