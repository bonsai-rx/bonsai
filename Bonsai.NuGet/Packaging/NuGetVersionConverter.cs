using System;
using System.ComponentModel;
using System.Globalization;
using NuGet.Versioning;

namespace Bonsai.NuGet.Packaging
{
    public class NuGetVersionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var text = value as string;
            if (text != null) return NuGetVersion.Parse(text);
            return base.ConvertFrom(context, culture, value);
        }
    }
}
