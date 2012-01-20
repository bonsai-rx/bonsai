using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    public class Undistort : Projection<IplImage, IplImage>
    {
        CvMat cameraMatrix;
        CvMat distortionCoefficients;

        public Undistort()
        {
            Distortion = new double[5];
        }

        public CvPoint2D64f FocalLength { get; set; }

        public CvPoint2D64f PrincipalPoint { get; set; }

        public double[] Distortion { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            ImgProc.cvUndistort2(input, output, cameraMatrix, distortionCoefficients, CvMat.Null);
            return output;
        }

        public override IDisposable Load()
        {
            var intrinsics = new double[] { FocalLength.X, 0, PrincipalPoint.X, 0, FocalLength.Y, PrincipalPoint.Y, 0, 0, 1 };
            cameraMatrix = new CvMat(3, 3, CvMatDepth.CV_64F, 1);
            Marshal.Copy(intrinsics, 0, cameraMatrix.Data, intrinsics.Length);

            distortionCoefficients = new CvMat(5, 1, CvMatDepth.CV_64F, 1);
            Marshal.Copy(Distortion, 0, distortionCoefficients.Data, Distortion.Length);
            return base.Load();
        }
    }
}
