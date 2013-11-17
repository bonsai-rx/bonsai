using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the absolute difference between the two input images.")]
    public class AbsoluteDifference : Transform<Tuple<IplImage, IplImage>, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input =>
            {
                var first = input.Item1;
                var second = input.Item2;
                var output = new IplImage(first.Size, first.Depth, first.Channels);
                CV.AbsDiff(first, second, output);
                return output;
            });
        }
    }
}
