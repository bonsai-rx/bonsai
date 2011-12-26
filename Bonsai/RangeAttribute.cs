using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RangeAttribute : Attribute
    {
        public static readonly RangeAttribute Default = new RangeAttribute(decimal.MinValue, decimal.MaxValue);

        public RangeAttribute(int min, int max)
            : this((decimal)min, (decimal)max)
        {
        }

        public RangeAttribute(float min, float max)
            : this((decimal)min, (decimal)max)
        {
        }

        public RangeAttribute(double min, double max)
            : this((decimal)min, (decimal)max)
        {
        }

        public RangeAttribute(decimal min, decimal max)
        {
            Minimum = min;
            Maximum = max;
        }

        public decimal Minimum { get; private set; }

        public decimal Maximum { get; private set; }
    }
}
