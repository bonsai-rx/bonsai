using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Resources
{
    public class ResourceFileNameConverter : StringConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
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
