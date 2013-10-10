using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Dsp
{
    public class Sum : Transform<Mat, Scalar>
    {
        public override IObservable<Scalar> Process(IObservable<Mat> source)
        {
            return Observable.Select(source, input => CV.Sum(input));
        }
    }
}
