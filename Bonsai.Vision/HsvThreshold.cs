using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    public class HsvThreshold : Transform<IplImage, IplImage>
    {
        public HsvThreshold()
        {
            Upper = new Scalar(179, 255, 255, 255);
        }

        [TypeConverter("Bonsai.Vision.Design.HsvScalarConverter, Bonsai.Vision.Design")]
        public Scalar Lower { get; set; }

        [TypeConverter("Bonsai.Vision.Design.HsvScalarConverter, Bonsai.Vision.Design")]
        public Scalar Upper { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.U8, 1);
                CV.InRangeS(input, Lower, Upper, output);
                return output;
            });
        }
    }
}
