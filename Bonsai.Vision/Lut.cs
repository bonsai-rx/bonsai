using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Performs a look-up table transformation of the input image pixels.")]
    public class Lut
    {
        public IObservable<IplImage> Process<TArray>(IObservable<Tuple<IplImage, TArray>> source) where TArray : Arr
        {
            return source.Select(input =>
            {
                var image = input.Item1;
                var lut = input.Item2;
                var output = new IplImage(image.Size, image.Depth, image.Channels);
                CV.LUT(image, output, lut);
                return output;
            });
        }
    }
}
