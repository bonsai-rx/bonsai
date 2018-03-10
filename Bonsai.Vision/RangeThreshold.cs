using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Tests which image elements lie within the specified range.")]
    public class RangeThreshold : Transform<IplImage, IplImage>
    {
        public RangeThreshold()
        {
            Upper = new Scalar(255, 255, 255, 255);
        }

        [TypeConverter(typeof(RangeScalarConverter))]
        [Description("The lower bound of the specified range.")]
        public Scalar Lower { get; set; }

        [TypeConverter(typeof(RangeScalarConverter))]
        [Description("The upper bound of the specified range.")]
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
