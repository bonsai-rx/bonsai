using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    class SourceNameArrayConverter : SourceNameConverter
    {
        static string RemoveWhiteSpace(string value)
        {
            return string.Join(string.Empty, value.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string[]) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var array = value as string[];
            if (array != null && destinationType == typeof(string))
            {
                return string.Join(", ", array);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var text = value as string;
            if (!string.IsNullOrEmpty(text))
            {
                return RemoveWhiteSpace(text).Split(',');
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
