using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Skeletonization : Selector<IplImage, IplImage>
    {
        IplImage distance;
        IplImage laplacian;

        public Skeletonization()
        {
            DistanceType = DistanceType.L2;
        }

        public DistanceType DistanceType { get; set; }

        public int LaplacianAperture { get; set; }

        [Precision(2, 0.1)]
        [Range(int.MinValue, 0.0)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public double RidgeThreshold { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, IplDepth.U8, 1);
            distance = IplImageHelper.EnsureImageFormat(distance, input.Size, IplDepth.F32, 1);
            laplacian = IplImageHelper.EnsureImageFormat(laplacian, input.Size, IplDepth.F32, 1);

            CV.DistTransform(input, distance, DistanceType);
            CV.Laplace(distance, laplacian, LaplacianAperture);
            CV.Threshold(laplacian, output, RidgeThreshold, 255, ThresholdTypes.BinaryInv);
            return output;
        }
    }
}
