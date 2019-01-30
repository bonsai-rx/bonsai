using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    public struct Intrinsics : IEquatable<Intrinsics>
    {
        public Size? ImageSize;
        public Point2d FocalLength;
        public Point2d PrincipalPoint;
        public Point3d RadialDistortion;
        public Point2d TangentialDistortion;

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

        public static Intrinsics FromCameraMatrix(Mat cameraMatrix, Mat distortionCoefficients, Size? imageSize)
        {
            Intrinsics intrinsics;
            FromCameraMatrix(cameraMatrix, distortionCoefficients, imageSize, out intrinsics);
            return intrinsics;
        }

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

        public bool Equals(Intrinsics other)
        {
            return ImageSize == other.ImageSize &&
                   FocalLength == other.FocalLength &&
                   PrincipalPoint == other.PrincipalPoint &&
                   RadialDistortion == other.RadialDistortion &&
                   TangentialDistortion == other.TangentialDistortion;
        }

        public override bool Equals(object obj)
        {
            if (obj is Intrinsics)
            {
                return Equals((Intrinsics)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ImageSize.GetHashCode() ^
                   FocalLength.GetHashCode() ^
                   PrincipalPoint.GetHashCode() ^
                   RadialDistortion.GetHashCode() ^
                   TangentialDistortion.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{ImageSize: {0}, FovY: {1}, FocalLength: {2}, PrincipalPoint: {3}, RadialDistortion: {4}, TangentialDistortion: {5}}}",
                ImageSize, FovY, FocalLength, PrincipalPoint, RadialDistortion, TangentialDistortion);
        }

        public static bool operator ==(Intrinsics left, Intrinsics right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Intrinsics left, Intrinsics right)
        {
            return !left.Equals(right);
        }
    }
}
