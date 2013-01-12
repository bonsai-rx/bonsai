using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Transpose : Transform<CvMat, CvMat>
    {
        public override CvMat Process(CvMat input)
        {
            var output = new CvMat(input.Rows, input.Cols, input.Depth, input.NumChannels);
            Core.cvTranspose(input, output);
            return output;
        }
    }
}
