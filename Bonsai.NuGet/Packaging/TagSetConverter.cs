using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Bonsai.NuGet.Packaging
{
    public class TagSetConverter : CommaDelimitedSetConverter
    {
        public override string SetSeparator => " ";

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is IEnumerable<string> set)
                value = set.Where(tag =>
                    tag != Constants.BonsaiDirectory &&
                    tag != Constants.GalleryDirectory);

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var result = base.ConvertFrom(context, culture, value);
            if (result is ISet<string> set)
            {
                set.Add(Constants.BonsaiDirectory);
                set.Add(Constants.GalleryDirectory);
            }

            return result;
        }
    }
}
