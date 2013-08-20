using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.IO
{
    public class ParseInt : Selector<string, CvMat>
    {
        public override CvMat Process(string input)
        {
            var output = new CvMat(1, 1, CvMatDepth.CV_32S, 1);
            Core.cvSet2D(output, 0, 0, CvScalar.Real(int.Parse(input)));
            return output;
        }
    }
}
