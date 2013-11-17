using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class CountNonZero : Transform<Mat, int>
    {
        public override IObservable<int> Process(IObservable<Mat> source)
        {
            return source.Select(CV.CountNonZero);
        }
    }
}
