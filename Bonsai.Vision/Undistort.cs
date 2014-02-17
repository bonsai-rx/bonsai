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
        Point2d focalLength;
        Point2d principalPoint;
        Point3d radialDistortion;
        Point2d tangentialDistortion;
        Mat intrinsics;
        Mat distortion;

        public Undistort()
        {
            UpdateIntrinsics();
            UpdateDistortion();
        }

        public Point2d FocalLength
        {
            get { return focalLength; }
            set
            {
                focalLength = value;
                UpdateIntrinsics();
            }
        }

        public Point2d PrincipalPoint
        {
            get { return principalPoint; }
            set
            {
                principalPoint = value;
                UpdateIntrinsics();
            }
        }

        public Point3d RadialDistortion
        {
            get { return radialDistortion; }
            set
            {
                radialDistortion = value;
                UpdateDistortion();
            }
        }

        public Point2d TangentialDistortion
        {
            get { return tangentialDistortion; }
            set
            {
                tangentialDistortion = value;
                UpdateDistortion();
            }
        }

        void UpdateIntrinsics()
        {
            intrinsics = Mat.FromArray(new double[,]
            {
                {focalLength.X, 0, principalPoint.X},
                {0, focalLength.Y, principalPoint.Y},
                {0, 0, 1}
            });
        }

        void UpdateDistortion()
        {
            distortion = Mat.FromArray(new[]
            {
                radialDistortion.X,
                radialDistortion.Y,
                tangentialDistortion.X,
                tangentialDistortion.Y,
                radialDistortion.Z
            });
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                return source.Select(input =>
                {
                    if (cameraMatrix != intrinsics)
                    {
                        cameraMatrix = intrinsics;
                    }

                    if (distortionCoefficients != distortion)
                    {
                        distortionCoefficients = distortion;
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Undistort2(input, output, cameraMatrix, distortionCoefficients);
                    return output;
                });
            });
        }
    }
}
