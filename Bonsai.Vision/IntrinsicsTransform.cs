using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class IntrinsicsTransform
    {
        double alpha;
        Size? imageSize;
        Point2d focalLength;
        Point2d principalPoint;
        Point3d radialDistortion;
        Point2d tangentialDistortion;
        Mat intrinsics;
        Mat distortion;

        protected IntrinsicsTransform()
        {
            UpdateDistortion();
            FocalLength = new Point2d(1, 1);
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The optional new image size used when computing the optimal camera matrix.")]
        public Size? ImageSize
        {
            get { return imageSize; }
            set
            {
                imageSize = value;
                UpdateIntrinsics();
            }
        }

        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
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

        protected Mat Intrinsics
        {
            get { return intrinsics; }
        }

        protected Mat Distortion
        {
            get { return distortion; }
        }

        protected void UpdateIntrinsics()
        {
            var cameraMatrix = Mat.FromArray(new double[,]
            {
                {focalLength.X, 0, principalPoint.X},
                {0, focalLength.Y, principalPoint.Y},
                {0, 0, 1}
            });

            var newSize = imageSize;
            if (newSize.HasValue)
            {
                var optimalMatrix = new Mat(cameraMatrix.Size, cameraMatrix.Depth, cameraMatrix.Channels);
                CV.GetOptimalNewCameraMatrix(cameraMatrix, distortion, newSize.Value, alpha, optimalMatrix);
                cameraMatrix = optimalMatrix;
            }
            intrinsics = cameraMatrix;
        }

        protected void UpdateDistortion()
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
    }
}
