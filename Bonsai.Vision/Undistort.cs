using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    public class Undistort : Transform<IplImage, IplImage>
    {
        public Undistort()
        {
            Distortion = new double[5];
        }

        public Point2d FocalLength { get; set; }

        public Point2d PrincipalPoint { get; set; }

        public double[] Distortion { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var intrinsics = new double[] { FocalLength.X, 0, PrincipalPoint.X, 0, FocalLength.Y, PrincipalPoint.Y, 0, 0, 1 };
                var cameraMatrix = new Mat(3, 3, Depth.F64, 1);
                Marshal.Copy(intrinsics, 0, cameraMatrix.Data, intrinsics.Length);

                var distortionCoefficients = new Mat(5, 1, Depth.F64, 1);
                Marshal.Copy(Distortion, 0, distortionCoefficients.Data, Distortion.Length);

                return source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Undistort2(input, output, cameraMatrix, distortionCoefficients);
                    return output;
                });
            });
        }
    }
}
