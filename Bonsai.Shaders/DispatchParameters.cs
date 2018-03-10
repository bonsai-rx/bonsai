using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(NumericRecordConverter))]
    public struct DispatchParameters : IEquatable<DispatchParameters>
    {
        public int NumGroupsX;
        public int NumGroupsY;
        public int NumGroupsZ;

        public bool Equals(DispatchParameters other)
        {
            return NumGroupsX == other.NumGroupsX &&
                   NumGroupsY == other.NumGroupsY &&
                   NumGroupsZ == other.NumGroupsZ;
        }

        public override bool Equals(object obj)
        {
            if (obj is DispatchParameters) return Equals((DispatchParameters)obj);
            else return false;
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
            return "NumGroups(" + NumGroupsX + "," + NumGroupsY + "," + NumGroupsZ + ")";
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
