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
                "{{ImageSize: {0}, FocalLength: {1}, PrincipalPoint: {2}, RadialDistortion: {3}, TangentialDistortion: {4}}}",
                ImageSize, FocalLength, PrincipalPoint, RadialDistortion, TangentialDistortion);
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
