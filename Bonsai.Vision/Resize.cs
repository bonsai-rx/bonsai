using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Resizes the input image.")]
    public class Resize : Transform<IplImage, IplImage>
    {
        public Resize()
        {
            Interpolation = SubPixelInterpolation.Linear;
        }

        [Description("The size of the output image.")]
        public Size Size { get; set; }

        [Description("The interpolation method used to transform individual image elements.")]
        public SubPixelInterpolation Interpolation { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                if (input.Size != Size)
                {
                    var output = new IplImage(Size, input.Depth, input.Channels);
                    CV.Resize(input, output, Interpolation);
                    return output;
                }
                else return input;
            });
        }
    }
}
