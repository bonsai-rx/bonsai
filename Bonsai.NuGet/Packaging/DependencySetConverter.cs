using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using NuGet.Packaging;

namespace Bonsai.NuGet.Packaging
{
    public class DependencySetConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "(Collection)";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var dependencyGroups = value as Collection<PackageDependencyGroup>;
            if (dependencyGroups != null)
            {
                var properties = from dependencyGroup in dependencyGroups
                                 from dependency in dependencyGroup.Packages
                                 select new ConstantPropertyDescriptor(dependency.Id, dependency.VersionRange);
                return new PropertyDescriptorCollection(properties.ToArray());
            }

            return base.GetProperties(context, value, attributes);
        }
    }
}
