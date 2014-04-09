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
    public class Skeletonization : Transform<IplImage, IplImage>
    {
        IplImage distance;
        IplImage laplacian;

        public Skeletonization()
        {
            LaplacianAperture = 3;
            DistanceType = DistanceType.L2;
        }

        [TypeConverter(typeof(DistanceTypeConverter))]
        public DistanceType DistanceType { get; set; }

        [Range(1, 31)]
        [Precision(0, 2)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int LaplacianAperture { get; set; }

        [Precision(2, 0.1)]
        [Range(int.MinValue, 0.0)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public double RidgeThreshold { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.U8, 1);
                distance = IplImageHelper.EnsureImageFormat(distance, input.Size, IplDepth.F32, 1);
                laplacian = IplImageHelper.EnsureImageFormat(laplacian, input.Size, IplDepth.F32, 1);

                CV.DistTransform(input, distance, DistanceType);
                CV.Laplace(distance, laplacian, LaplacianAperture);
                CV.Threshold(laplacian, output, RidgeThreshold, 255, ThresholdTypes.BinaryInv);
                return output;
            });
        }
    }
}
