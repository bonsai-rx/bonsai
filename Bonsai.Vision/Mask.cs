using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Mask : Transform<Tuple<IplImage, IplImage>, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input =>
            {
                var image = input.Item1;
                var mask = input.Item2;
                var output = new IplImage(image.Size, image.Depth, image.Channels);
                output.SetZero();
                CV.Copy(image, output, mask);
                return output;
            });
        }
    }
}
