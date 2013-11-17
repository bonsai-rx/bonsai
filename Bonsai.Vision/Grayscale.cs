using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Converts a BGR color image to grayscale.")]
    public class Grayscale : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                if (input.Channels == 1)
                {
                    return input;
                }
                else
                {
                    var output = new IplImage(input.Size, IplDepth.U8, 1);
                    CV.CvtColor(input, output, ColorConversion.Bgr2Gray);
                    return output;
                }
            });
        }
    }
}
