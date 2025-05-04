using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Bonsai.NuGet.Packaging
{
    public class CommaDelimitedSetConverter : TypeConverter
    {
        public virtual string SetSeparator => ",";

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is IEnumerable<string> set)
                return string.Join(SetSeparator, set);

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string text)
            {
                var names = text.Split(new[] { SetSeparator }, StringSplitOptions.RemoveEmptyEntries);
                var set = context.PropertyDescriptor.GetValue(context.Instance) as ISet<string>;
                if (set != null)
                {
                    set.Clear();
                    foreach (var name in names)
                    {
                        set.Add(name);
                    }
                }

                return set;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
