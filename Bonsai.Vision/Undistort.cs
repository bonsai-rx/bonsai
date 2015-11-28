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
    [Description("Undistorts the input image using the specified intrinsic camera matrix.")]
    public class Undistort : Transform<IplImage, IplImage>
    {
        double alpha;
        bool computeOptimalMatrix;
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

        [Range(0, 1)]
        [Precision(2, 0.01)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("The free scaling parameter used when computing the optimal camera matrix.")]
        public double Alpha
        {
            get { return alpha; }
            set
            {
                alpha = value;
                UpdateIntrinsics();
            }
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

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                Mat mapX = null;
                Mat mapY = null;
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                return source.Select(input =>
                {
                    if (mapX == null || mapY == null || mapX.Size != input.Size)
                    {
                        mapX = new Mat(input.Size, Depth.F32, 1);
                        mapY = new Mat(input.Size, Depth.F32, 1);
                    }

                    if (cameraMatrix != intrinsics || distortionCoefficients != distortion)
                    {
                        cameraMatrix = intrinsics;
                        distortionCoefficients = distortion;
                        var optimalMatrix = cameraMatrix;
                        if (computeOptimalMatrix)
                        {
                            optimalMatrix = new Mat(cameraMatrix.Size, cameraMatrix.Depth, cameraMatrix.Channels);
                            CV.GetOptimalNewCameraMatrix(cameraMatrix, distortionCoefficients, input.Size, alpha, optimalMatrix);
                        }
                        CV.InitUndistortRectifyMap(cameraMatrix, distortionCoefficients, null, optimalMatrix, mapX, mapY);
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Remap(input, output, mapX, mapY);
                    return output;
                });
            });
        }
    }
}
