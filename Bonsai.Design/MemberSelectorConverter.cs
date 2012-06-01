using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Bonsai.Design
{
    public class MemberSelectorConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var text = value as string;
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var memberChain = value as string[];
            if (destinationType == typeof(string) && memberChain != null)
            {
                return string.Join(".", memberChain);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
