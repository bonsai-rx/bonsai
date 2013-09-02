using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the sum of image elements independently for each color channel.")]
    public class Sum : Selector<IplImage, Scalar>
    {
        public override Scalar Process(IplImage input)
        {
            return CV.Sum(input);
        }
    }
}
