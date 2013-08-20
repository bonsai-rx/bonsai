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

        public CvPoint2D64f FocalLength { get; set; }

        public CvPoint2D64f PrincipalPoint { get; set; }

        public double[] Distortion { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>(observer =>
            {
                var intrinsics = new double[] { FocalLength.X, 0, PrincipalPoint.X, 0, FocalLength.Y, PrincipalPoint.Y, 0, 0, 1 };
                var cameraMatrix = new CvMat(3, 3, CvMatDepth.CV_64F, 1);
                Marshal.Copy(intrinsics, 0, cameraMatrix.Data, intrinsics.Length);

                var distortionCoefficients = new CvMat(5, 1, CvMatDepth.CV_64F, 1);
                Marshal.Copy(Distortion, 0, distortionCoefficients.Data, Distortion.Length);

                var process = source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    ImgProc.cvUndistort2(input, output, cameraMatrix, distortionCoefficients, CvMat.Null);
                    return output;
                }).Subscribe(observer);

                return process;
            });
        }
    }
}
