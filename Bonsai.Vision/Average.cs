using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the average (mean) of the image elements for each color channel.")]
    public class Average : Transform<IplImage, Scalar>
    {
        public override IObservable<Scalar> Process(IObservable<IplImage> source)
        {
            return source.Select(input => CV.Avg(input));
        }
    }
}
