using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Laplace : Transform<IplImage, IplImage>
    {
        public Laplace()
        {
            ApertureSize = 3;
        }

        public int ApertureSize { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.Laplace(input, output, ApertureSize);
                return output;
            });
        }
    }
}
