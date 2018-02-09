using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Extrinsics : IEquatable<Extrinsics>
    {
        public Point3d Rotation;
        public Point3d Translation;

        public bool Equals(Extrinsics other)
        {
            return Rotation == other.Rotation && Translation == other.Translation;
        }

        public override bool Equals(object obj)
        {
            if (obj is Extrinsics)
            {
                return Equals((Extrinsics)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Rotation.GetHashCode() ^ Translation.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{{Rotation: {0}, Translation: {1}}}", Rotation, Translation);
        }

        public static bool operator ==(Extrinsics left, Extrinsics right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Extrinsics left, Extrinsics right)
        {
            return !left.Equals(right);
        }
    }
}
