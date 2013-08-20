using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class Normalize : Selector<IplImage, IplImage>
    {
        public override IplImage Process(IplImage input)
        {
            double min, max;
            CvPoint minLoc, maxLoc;
            var output = new IplImage(input.Size, 8, input.NumChannels);
            Core.cvMinMaxLoc(input, out min, out max, out minLoc, out maxLoc, CvArr.Null);

            var range = max - min;
            var scale = range > 0 ? 255.0 / range : 0;
            var shift = range > 0 ? -min : 0;
            Core.cvConvertScale(input, output, scale, shift);
            return output;
        }
    }
}
