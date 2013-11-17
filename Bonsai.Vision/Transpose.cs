using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Transpose : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(new Size(input.Height, input.Width), input.Depth, input.Channels);
                CV.Transpose(input, output);
                return output;
            });
        }
    }
}
