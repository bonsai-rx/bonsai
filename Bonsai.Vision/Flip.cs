using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Flips the input image around vertical, horizontal or both axes.")]
    public class Flip : Transform<IplImage, IplImage>
    {
        [Description("Specifies how to flip the image.")]
        public FlipMode Mode { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.Flip(input, output, Mode);
                return output;
            });
        }
    }
}
