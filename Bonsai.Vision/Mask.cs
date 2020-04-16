using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Fills all image pixels that are not in the input mask with the specified value.")]
    public class Mask : Transform<Tuple<IplImage, IplImage>, IplImage>
    {
        [Description("The value to which all pixels that are not in the input mask will be set to.")]
        public Scalar FillValue { get; set; }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input =>
            {
                var image = input.Item1;
                var mask = input.Item2;
                var output = new IplImage(image.Size, image.Depth, image.Channels);
                output.Set(FillValue);
                CV.Copy(image, output, mask);
                return output;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage, IplImage>> source)
        {
            return source.Select(input =>
            {
                var image = input.Item1;
                var mask = input.Item2;
                var output = input.Item3.Clone();
                CV.Copy(image, output, mask);
                return output;
            });
        }
    }
}
