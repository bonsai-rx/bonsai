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
                    tag != Constants.BonsaiTag &&
                    tag != Constants.GalleryTag);

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var result = base.ConvertFrom(context, culture, value);
            if (result is ISet<string> set)
            {
                set.Add(Constants.BonsaiTag);
                set.Add(Constants.GalleryTag);
            }

            return result;
        }
    }
}
