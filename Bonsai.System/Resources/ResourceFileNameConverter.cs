using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Bonsai.Resources
{
    /// <summary>
    /// Provides a type converter to convert a resource file name to and from other representations.
    /// </summary>
    public class ResourceFileNameConverter : StringConverter
    {
        /// <summary>
        /// Converts the given <paramref name="value"/> object to a resource file name,
        /// and also sets the <c>Name</c> property of the object that is connected to this type
        /// converter request, if it exists.
        /// </summary>
        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var instance = context.Instance;
            var fileName = value as string;
            if (!string.IsNullOrWhiteSpace(fileName) && instance != null)
            {
                var properties = TypeDescriptor.GetProperties(instance);
                var nameProperty = properties.Find("Name", false);
                if (nameProperty != null && nameProperty.PropertyType == typeof(string))
                {
                    var name = nameProperty.GetValue(instance) as string;
                    if (string.IsNullOrEmpty(name))
                    {
                        nameProperty.SetValue(instance, Path.GetFileNameWithoutExtension(fileName));
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
