using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.IO
{
    public class ParseInt : Selector<string, Mat>
    {
        public override Mat Process(string input)
        {
            var output = new Mat(1, 1, Depth.S32, 1);
            output[0, 0] = Scalar.Real(int.Parse(input));
            return output;
        }
    }
}
