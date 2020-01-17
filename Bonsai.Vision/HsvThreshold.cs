using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Segments the input image using an HSV color range.")]
    public class HsvThreshold : Transform<IplImage, IplImage>
    {
        public HsvThreshold()
        {
            Upper = new Scalar(179, 255, 255, 255);
        }

        [TypeConverter(typeof(HsvScalarConverter))]
        [Description("The lower bound of the HSV color range.")]
        public Scalar Lower { get; set; }

        [TypeConverter(typeof(HsvScalarConverter))]
        [Description("The upper bound of the HSV color range.")]
        public Scalar Upper { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var lower = Lower;
                var upper = Upper;
                var output = new IplImage(input.Size, IplDepth.U8, 1);
                if (upper.Val0 < lower.Val0)
                {
                    var upperH = new Scalar(180, upper.Val1, upper.Val2, upper.Val3);
                    var lowerH = new Scalar(0, lower.Val1, lower.Val2, lower.Val3);
                    using (var temp = new IplImage(input.Size, IplDepth.U8, 1))
                    {
                        CV.InRangeS(input, lower, upperH, temp);
                        CV.InRangeS(input, lowerH, upper, output);
                        CV.Or(temp, output, output);
                    }
                }
                else CV.InRangeS(input, lower, upper, output);
                return output;
            });
        }
    }
}
