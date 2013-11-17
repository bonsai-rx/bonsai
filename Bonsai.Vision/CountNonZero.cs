using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class CountNonZero : Transform<IplImage, int>
    {
        public override IObservable<int> Process(IObservable<IplImage> source)
        {
            return source.Select(CV.CountNonZero);
        }
    }
}
