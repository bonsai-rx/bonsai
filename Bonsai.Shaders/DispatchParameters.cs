using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Bonsai.Shaders
{
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(NumericRecordConverter))]
    public struct DispatchParameters : IEquatable<DispatchParameters>
    {
        public int NumGroupsX;
        public int NumGroupsY;
        public int NumGroupsZ;

        public DispatchParameters(int numGroupsX, int numGroupsY, int numGroupsZ)
        {
            NumGroupsX = numGroupsX;
            NumGroupsY = numGroupsY;
            NumGroupsZ = numGroupsZ;
        }

        public bool Equals(DispatchParameters other)
        {
            return NumGroupsX == other.NumGroupsX &&
                   NumGroupsY == other.NumGroupsY &&
                   NumGroupsZ == other.NumGroupsZ;
        }

        public override bool Equals(object obj)
        {
            return obj is DispatchParameters && Equals((DispatchParameters)obj);
        }

        public override int GetHashCode()
        {
            var hash = 53;
            hash += 97 * NumGroupsX.GetHashCode();
            hash += 97 * NumGroupsY.GetHashCode();
            hash += 97 * NumGroupsZ.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"NumGroups({NumGroupsX}, {NumGroupsY}, {NumGroupsZ})";
        }

        public static bool operator ==(DispatchParameters left, DispatchParameters right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DispatchParameters left, DispatchParameters right)
        {
            return !left.Equals(right);
        }
    }
}
