using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    class KernelLengthConverter : Int32Converter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var kernelLength = (int)base.ConvertFrom(context, culture, value);
            if (kernelLength % 2 != 0)
            {
                throw new ArgumentOutOfRangeException("value", "The length of the filter kernel must be an even number.");
            }

            return kernelLength;
        }
    }
}
