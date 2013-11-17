using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class SelectChannels : Transform<Mat, Mat>
    {
        public SelectChannels()
        {
            Step = 1;
        }

        public int Start { get; set; }

        public int Stop { get; set; }

        public int Step { get; set; }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input => input.GetRows(Start, Stop, Step));
        }
    }
}
