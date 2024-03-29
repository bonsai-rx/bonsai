﻿using System;
using System.ComponentModel;

namespace Bonsai.Vision
{
    class OddKernelSizeConverter : Int32Converter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var kernelSize = (int)base.ConvertFrom(context, culture, value);
            if (kernelSize % 2 == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The size of the filter kernel must be an odd number.");
            }

            return kernelSize;
        }
    }
}
