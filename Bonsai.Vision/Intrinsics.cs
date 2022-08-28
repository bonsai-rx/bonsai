using OpenCV.Net;
using System;
using System.Globalization;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents parameters that describe the camera intrinsic properties
    /// such as the focal length or lens distortion.
    /// </summary>
    public struct Intrinsics : IEquatable<Intrinsics>
    {
        /// <summary>
        /// The image size of the camera, in pixels.
        /// </summary>
        public Size? ImageSize;

        /// <summary>
        /// The focal length of the camera.
        /// </summary>
        public Point2d FocalLength;

        /// <summary>
        /// The principal point of the camera.
        /// </summary>
        public Point2d PrincipalPoint;

        /// <summary>
        /// The radial distortion coefficients of the camera.
        /// </summary>
        public Point3d RadialDistortion;

        /// <summary>
        /// The tangential distortion coefficients of the camera.
        /// </summary>
        public Point2d TangentialDistortion;

        /// <summary>
        /// Gets the vertical field of view of the camera.
        /// </summary>
        public double? FovY
        {
            get
            {
                var imageSize = ImageSize.GetValueOrDefault();
                if (imageSize.Height > 0)
                {
                    return 2 * Math.Atan(0.5 * imageSize.Height / FocalLength.Y);
                }
                else return null;
            }
        }

        /// <summary>
        /// Returns an <see cref="Intrinsics"/> structure representing the camera
        /// intrinsic parameters extracted from a camera matrix, lens distortion
        /// and optional image size.
        /// </summary>
        /// <param name="cameraMatrix">
        /// A 2x3 matrix specifying the focal lengths and principal point offset.
        /// </param>
        /// <param name="distortionCoefficients">
        /// A 1x5 or 5x1 vector specifying the coefficients for the lens distortion
        /// model.
        /// </param>
        /// <param name="imageSize">The image size of the camera, in pixels.</param>
        /// <returns>
        /// An <see cref="Intrinsics"/> object representing the extracted camera
        /// intrinsic parameters.
        /// </returns>
        public static Intrinsics FromCameraMatrix(Mat cameraMatrix, Mat distortionCoefficients, Size? imageSize)
        {
            FromCameraMatrix(cameraMatrix, distortionCoefficients, imageSize, out Intrinsics intrinsics);
            return intrinsics;
        }

        /// <summary>
        /// Initializes an <see cref="Intrinsics"/> structure representing the camera
        /// intrinsic parameters extracted from a camera matrix, lens distortion
        /// and optional image size.
        /// </summary>
        /// <param name="cameraMatrix">
        /// A 2x3 matrix specifying the focal lengths and principal point offset.
        /// </param>
        /// <param name="distortionCoefficients">
        /// A 1x5 or 5x1 vector specifying the coefficients for the lens distortion
        /// model.
        /// </param>
        /// <param name="imageSize">The image size of the camera, in pixels.</param>
        /// <param name="intrinsics">
        /// When this method returns, contains an <see cref="Intrinsics"/> object
        /// representing the extracted camera intrinsic parameters.
        /// </param>
        public static void FromCameraMatrix(Mat cameraMatrix, Mat distortionCoefficients, Size? imageSize, out Intrinsics intrinsics)
        {
            intrinsics.ImageSize = imageSize;
            if (cameraMatrix != null)
            {
                var fx = cameraMatrix.GetReal(0, 0);
                var fy = cameraMatrix.GetReal(1, 1);
                var px = cameraMatrix.GetReal(0, 2);
                var py = cameraMatrix.GetReal(1, 2);
                intrinsics.FocalLength = new Point2d(fx, fy);
                intrinsics.PrincipalPoint = new Point2d(px, py);
            }
            else
            {
                intrinsics.FocalLength = Point2d.Zero;
                intrinsics.PrincipalPoint = Point2d.Zero;
            }

            if (distortionCoefficients != null)
            {
                var d0 = distortionCoefficients.GetReal(0);
                var d1 = distortionCoefficients.GetReal(1);
                var d2 = distortionCoefficients.GetReal(2);
                var d3 = distortionCoefficients.GetReal(3);
                var d4 = distortionCoefficients.GetReal(4);
                intrinsics.RadialDistortion = new Point3d(d0, d1, d4);
                intrinsics.TangentialDistortion = new Point2d(d2, d3);
            }
            else
            {
                intrinsics.RadialDistortion = Point3d.Zero;
                intrinsics.TangentialDistortion = Point2d.Zero;
            }
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to the
        /// specified <see cref="Intrinsics"/> structure.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Intrinsics"/> object to compare with this instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> represents
        /// the same parameter values as this instance; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(Intrinsics other)
        {
            return ImageSize == other.ImageSize &&
                   FocalLength == other.FocalLength &&
                   PrincipalPoint == other.PrincipalPoint &&
                   RadialDistortion == other.RadialDistortion &&
                   TangentialDistortion == other.TangentialDistortion;
        }

        /// <summary>
        /// Returns a value indicating whether the specified object is an <see cref="Intrinsics"/>
        /// structure with the same parameter values as this <see cref="Intrinsics"/> structure.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is an <see cref="Intrinsics"/>
        /// structure and has the same parameter values as this structure; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Intrinsics intrinsics)
            {
                return Equals(intrinsics);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="Intrinsics"/> structure.
        /// </summary>
        /// <returns>
        /// An integer value that specifies a hash value for this
        /// <see cref="Intrinsics"/> structure.
        /// </returns>
        public override int GetHashCode()
        {
            return ImageSize.GetHashCode() ^
                   FocalLength.GetHashCode() ^
                   PrincipalPoint.GetHashCode() ^
                   RadialDistortion.GetHashCode() ^
                   TangentialDistortion.GetHashCode();
        }

        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="Intrinsics"/> structure.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing all the parameter values of this
        /// <see cref="Intrinsics"/> structure.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{ImageSize: {0}, FovY: {1}, FocalLength: {2}, PrincipalPoint: {3}, RadialDistortion: {4}, TangentialDistortion: {5}}}",
                ImageSize, FovY, FocalLength, PrincipalPoint, RadialDistortion, TangentialDistortion);
        }

        /// <summary>
        /// Indicates whether two <see cref="Intrinsics"/> structures are equal.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Intrinsics"/> structure on the left-hand side of the
        /// equality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="Intrinsics"/> structure on the right-hand side of the
        /// equality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// have equal parameter values; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Intrinsics left, Intrinsics right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two <see cref="Intrinsics"/> structures are different.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Intrinsics"/> structure on the left-hand side of the
        /// inequality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="Intrinsics"/> structure on the right-hand side of the
        /// inequality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// differ in any of their parameter values; <see langword="false"/> if
        /// <paramref name="left"/> and <paramref name="right"/> are equal.
        /// </returns>
        public static bool operator !=(Intrinsics left, Intrinsics right)
        {
            return !left.Equals(right);
        }
    }
}
