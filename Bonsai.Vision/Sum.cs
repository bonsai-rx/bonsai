using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the sum of image elements independently for each color channel.")]
    public class Sum : Transform<IplImage, Scalar>
    {
        public override IObservable<Scalar> Process(IObservable<IplImage> source)
        {
            return source.Select(CV.Sum);
        }
    }
}
