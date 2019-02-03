using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public struct TimeStep : IEquatable<TimeStep>
    {
        public double ElapsedTime;
        public double ElapsedRealTime;

        public TimeStep(double elapsedTime, double elapsedRealTime)
        {
            ElapsedTime = elapsedTime;
            ElapsedRealTime = elapsedRealTime;
        }

        public bool Equals(TimeStep other)
        {
            return ElapsedTime == other.ElapsedTime && ElapsedRealTime == other.ElapsedRealTime;
        }

        public override bool Equals(object obj)
        {
            if (obj is TimeStep)
            {
                return Equals((TimeStep)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hash = 82613;
            hash = hash * 103787 + EqualityComparer<double>.Default.GetHashCode(ElapsedTime);
            hash = hash * 103787 + EqualityComparer<double>.Default.GetHashCode(ElapsedRealTime);
            return hash;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{ElapsedTime: {0}, ElapsedRealTime: {1}}}",
                ElapsedTime, ElapsedRealTime);
        }

        public static void Add(ref TimeStep left, ref TimeStep right, out TimeStep result)
        {
            result.ElapsedTime = left.ElapsedTime + right.ElapsedTime;
            result.ElapsedRealTime = left.ElapsedRealTime + right.ElapsedRealTime;
        }

        public static void Subtract(ref TimeStep left, ref TimeStep right, out TimeStep result)
        {
            result.ElapsedTime = left.ElapsedTime - right.ElapsedTime;
            result.ElapsedRealTime = left.ElapsedRealTime - right.ElapsedRealTime;
        }

        public static TimeStep Add(TimeStep left, TimeStep right)
        {
            TimeStep result;
            Add(ref left, ref right, out result);
            return result;
        }

        public static TimeStep Subtract(TimeStep left, TimeStep right)
        {
            TimeStep result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        public static bool operator ==(TimeStep left, TimeStep right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeStep left, TimeStep right)
        {
            return !left.Equals(right);
        }

        public static TimeStep operator +(TimeStep left, TimeStep right)
        {
            return Add(left, right);
        }

        public static TimeStep operator -(TimeStep left, TimeStep right)
        {
            return Subtract(left, right);
        }
    }
}
