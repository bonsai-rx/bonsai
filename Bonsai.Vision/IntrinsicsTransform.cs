using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Provides an abstract base class for all operators requiring a specified set of
    /// camera intrinsics and distortion parameters.
    /// </summary>
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

        internal IntrinsicsTransform()
        {
            UpdateDistortion();
            FocalLength = new Point2d(1, 1);
        }

        /// <summary>
        /// Gets or sets the image size used when computing the optimal camera matrix.
        /// </summary>
        /// <remarks>
        /// If the image size is specified, the optimal camera matrix is estimated
        /// and used to scale the camera intrinsics in such a way as to avoid losing
        /// pixels which would be lost when undistorting the original frames.
        /// </remarks>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The image size used when computing the optimal camera matrix.")]
        public Size? ImageSize
        {
            get { return imageSize; }
            set
            {
                imageSize = value;
                UpdateIntrinsics();
            }
        }

        /// <summary>
        /// Gets or sets the free scaling parameter used when computing the optimal
        /// camera matrix.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the focal length of the camera, expressed in pixel units.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the principal point of the camera, usually at the image center.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the radial distortion coefficients.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the tangential distortion coefficients.
        /// </summary>
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

        /// <summary>
        /// Gets the full camera intrinsics matrix.
        /// </summary>
        protected Mat Intrinsics
        {
            get { return intrinsics; }
        }

        /// <summary>
        /// Gets the matrix of camera distortion coefficients.
        /// </summary>
        protected Mat Distortion
        {
            get { return distortion; }
        }

        internal void UpdateIntrinsics()
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

        internal void UpdateDistortion()
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
