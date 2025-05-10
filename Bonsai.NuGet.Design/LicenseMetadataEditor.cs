using System;
using System.ComponentModel;
using Bonsai.Design;
using NuGet.Packaging;

namespace Bonsai.NuGet.Design
{
    class LicenseMetadataEditor : OpenFileNameEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var newValue = base.EditValue(context, provider, value);
            if (!ReferenceEquals(newValue, value) && newValue is string license)
            {
                return new LicenseMetadata(
                    LicenseType.File,
                    license,
                    expression: default,
                    warningsAndErrors: Array.Empty<string>(),
                    LicenseMetadata.CurrentVersion);
            }

            return newValue;
        }
    }
}
