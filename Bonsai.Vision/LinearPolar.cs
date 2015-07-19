using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Performs forward or inverse linear-polar image transform. This transform \"emulates\" human foveal vision.")]
    public class LinearPolar : Transform<IplImage, IplImage>
    {
        public LinearPolar()
        {
            Flags = WarpFlags.Linear | WarpFlags.FillOutliers;
        }

        [Description("The transformation center where the output precision is maximal.")]
        public Point2f Center { get; set; }

        [Description("The maximum radius of polar transformation.")]
        public double MaxRadius { get; set; }

        [Description("Specifies interpolation and operation flags for the image warp.")]
        public WarpFlags Flags { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.LinearPolar(input, output, Center, MaxRadius, Flags);
                return output;
            });
        }
    }
}
