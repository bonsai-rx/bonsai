using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the distance to the closest zero pixel for all non-zero pixels of the input image.")]
    public class DistanceTransform : Transform<IplImage, IplImage>
    {
        public DistanceTransform()
        {
            DistanceType = OpenCV.Net.DistanceType.L2;
        }

        [TypeConverter(typeof(DistanceTypeConverter))]
        [Description("The type of distance function to use.")]
        public DistanceType DistanceType { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.F32, 1);
                CV.DistTransform(input, output, DistanceType);
                return output;
            });
        }
    }
}
