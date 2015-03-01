using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    [Description("Normalizes the range of the input image to be between zero and one.")]
    public class Normalize : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                double min, max;
                Point minLoc, maxLoc;
                var output = new IplImage(input.Size, IplDepth.F32, input.Channels);
                CV.MinMaxLoc(input, out min, out max, out minLoc, out maxLoc);

                var range = max - min;
                var scale = range > 0 ? 1.0 / range : 0;
                var shift = range > 0 ? -min : 0;
                CV.ConvertScale(input, output, scale, shift);
                return output;
            });
        }
    }
}
