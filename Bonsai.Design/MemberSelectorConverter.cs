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
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var memberChain = value as Collection<string>;
            if (destinationType == typeof(string) && memberChain != null)
            {
                return string.Join(".", memberChain.ToArray());
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
