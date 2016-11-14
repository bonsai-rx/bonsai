using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    [Description("Undistorts the observed point coordinates using the specified intrinsic camera matrix.")]
    public class UndistortPoints : Transform<Mat, Mat>
    {
        bool computeOptimalMatrix;
        Point2d focalLength;
        Point2d principalPoint;
        Point3d radialDistortion;
        Point2d tangentialDistortion;
        Mat intrinsics;
        Mat distortion;

        public UndistortPoints()
        {
            UpdateIntrinsics();
            UpdateDistortion();
        }

        [Description("Specifies whether to compute the optimal camera matrix for the specified distortion parameters.")]
        public bool ComputeOptimalMatrix
        {
            get { return computeOptimalMatrix; }
            set
            {
                computeOptimalMatrix = value;
                UpdateIntrinsics();
            }
        }

        [Description("The focal length of the camera, expressed in pixel units.")]
        public Point2d FocalLength
        {
            get { return focalLength; }
            set
            {
                focalLength = value;
                UpdateIntrinsics();
            }
        }

        [Description("The principal point of the camera, usually at the image center.")]
        public Point2d PrincipalPoint
        {
            get { return principalPoint; }
            set
            {
                principalPoint = value;
                UpdateIntrinsics();
            }
        }

        [Description("The radial distortion coefficients.")]
        public Point3d RadialDistortion
        {
            get { return radialDistortion; }
            set
            {
                radialDistortion = value;
                UpdateDistortion();
            }
        }

        [Description("The tangential distortion coefficients.")]
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

        public IObservable<Point2f> Process(IObservable<Point2f> source)
        {
            return Process(source.Select(x => new[] { x })).Select(xs => xs[0]);
        }

        public IObservable<Point2f[]> Process(IObservable<Point2f[]> source)
        {
            return Observable.Defer(() =>
            {
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                return source.Select(input =>
                {
                    if (cameraMatrix != intrinsics || distortionCoefficients != distortion)
                    {
                        cameraMatrix = intrinsics;
                        distortionCoefficients = distortion;
                    }

                    var output = new Point2f[input.Length];
                    using (var inputHeader = Mat.CreateMatHeader(input, input.Length, 2, Depth.F32, 1))
                    using (var outputHeader = Mat.CreateMatHeader(output, output.Length, 2, Depth.F32, 1))
                    {
                        CV.UndistortPoints(inputHeader, outputHeader, cameraMatrix, distortionCoefficients);
                    }
                    return output;
                });
            });
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                return source.Select(input =>
                {
                    if (cameraMatrix != intrinsics || distortionCoefficients != distortion)
                    {
                        cameraMatrix = intrinsics;
                        distortionCoefficients = distortion;
                    }

                    var output = new Mat(input.Size, input.Depth, input.Channels);
                    CV.UndistortPoints(input, output, cameraMatrix, distortionCoefficients);
                    return output;
                });
            });
        }
    }
}
