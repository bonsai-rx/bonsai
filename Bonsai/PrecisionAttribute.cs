using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PrecisionAttribute : Attribute
    {
        public static readonly PrecisionAttribute Default = new PrecisionAttribute(0, 1);

        public PrecisionAttribute(int decimalPlaces, int increment)
            : this(decimalPlaces, (decimal)increment)
        {
        }

        public PrecisionAttribute(int decimalPlaces, double increment)
            : this(decimalPlaces, (decimal)increment)
        {
        }

        public PrecisionAttribute(int decimalPlaces, decimal increment)
        {
            DecimalPlaces = decimalPlaces;
            Increment = increment;
        }

        public int DecimalPlaces { get; private set; }

        public decimal Increment { get; private set; }
    }
}
