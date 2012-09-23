using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the sum of image elements independently for each color channel.")]
    public class Sum : Projection<IplImage, CvScalar>
    {
        public override CvScalar Process(IplImage input)
        {
            return Core.cvSum(input);
        }
    }
}
