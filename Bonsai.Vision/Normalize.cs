using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class Normalize : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                double min, max;
                Point minLoc, maxLoc;
                var output = new IplImage(input.Size, IplDepth.U8, input.Channels);
                CV.MinMaxLoc(input, out min, out max, out minLoc, out maxLoc);

                var range = max - min;
                var scale = range > 0 ? 255.0 / range : 0;
                var shift = range > 0 ? -min : 0;
                CV.ConvertScale(input, output, scale, shift);
                return output;
            });
        }
    }
}
