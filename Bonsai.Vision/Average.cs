using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the average (mean) of the image elements for each color channel.")]
    public class Average : Selector<IplImage, Scalar>
    {
        public override Scalar Process(IplImage input)
        {
            return CV.Avg(input);
        }
    }
}
