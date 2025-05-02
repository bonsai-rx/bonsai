using System;
using System.ComponentModel;
using System.Globalization;
using NuGet.Packaging;
using NuGet.Packaging.Licenses;

namespace Bonsai.NuGet.Design
{
    class LicenseMetadataConverter : TypeConverter
    {
        const string ExpressionPrefix = "spdx:";
        const string FilePrefix = "file:";

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var license = value as string;
            if (!string.IsNullOrEmpty(license))
            {
                LicenseType type;
                NuGetLicenseExpression expression;
                if (license.StartsWith(FilePrefix))
                {
                    license = license.Substring(FilePrefix.Length);
                    type = LicenseType.File;
                    expression = default;
                }
                else
                {
                    type = LicenseType.Expression;
                    if (license.StartsWith(ExpressionPrefix))
                        license = license.Substring(ExpressionPrefix.Length);
                    expression = NuGetLicenseExpression.Parse(license);
                }
                return new LicenseMetadata(type, license, expression, null, LicenseMetadata.CurrentVersion);
            }

            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is LicenseMetadata licenseMetadata)
            {
                var prefix = licenseMetadata.Type switch
                {
                    LicenseType.Expression => ExpressionPrefix,
                    LicenseType.File => FilePrefix,
                    _ => throw new InvalidOperationException("Invalid license type.")
                };
                return $"{prefix}{licenseMetadata.License}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
