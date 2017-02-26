using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class QuaternionConverter : TypeConverter
    {
        static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static IEnumerable<PropertyDescriptor> GetProperties(Type type)
        {
            return TypeDescriptor.GetProperties(type)
                                 .Cast<PropertyDescriptor>()
                                 .Where(property => !property.IsReadOnly && !string.Equals(property.Name, "xyz", StringComparison.OrdinalIgnoreCase));
        }

        static Type GetUnderlyingType(ITypeDescriptorContext context)
        {
            var type = context.PropertyDescriptor.PropertyType;
            if (IsNullable(type))
            {
                type = type.GetGenericArguments()[0];
            }

            return type;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var valueString = value as string;
            if (valueString != null)
            {
                valueString = valueString.Trim();
                if (!string.IsNullOrEmpty(valueString))
                {
                    var type = GetUnderlyingType(context);
                    var properties = GetProperties(type).ToArray();
                    var propertyValues = valueString.Split(new[] { culture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    if (propertyValues.Length == properties.Length)
                    {
                        var instance = Activator.CreateInstance(type);
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var propertyValue = Convert.ChangeType(propertyValues[i], properties[i].PropertyType, culture);
                            properties[i].SetValue(instance, propertyValue);
                        }

                        return instance;
                    }
                }
                else if (IsNullable(context.PropertyDescriptor.PropertyType))
                {
                    return null;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {
                var type = GetUnderlyingType(context);
                var properties = GetProperties(type);
                var propertyValues = properties.Select(property => Convert.ToString(property.GetValue(value), culture));
                return string.Join(culture.TextInfo.ListSeparator, propertyValues);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            var type = GetUnderlyingType(context);
            var properties = GetProperties(type);
            var instance = Activator.CreateInstance(type);
            foreach (var property in properties)
            {
                var value = Convert.ChangeType(propertyValues[property.Name], property.PropertyType);
                property.SetValue(instance, value);
            }

            return instance;
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (value == null) return null;
            var type = GetUnderlyingType(context);
            var properties = GetProperties(type).ToArray();
            var propertyNames = Array.ConvertAll(properties, property => property.Name);
            return new PropertyDescriptorCollection(properties).Sort(propertyNames);
        }
    }
}
