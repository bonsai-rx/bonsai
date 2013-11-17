using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using Bonsai;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the per-element bitwise inversion of the input image.")]
    public class Not : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.Not(input, output);
                return output;
            });
        }
    }
}
